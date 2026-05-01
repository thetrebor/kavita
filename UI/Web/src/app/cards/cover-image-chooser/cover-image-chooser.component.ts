import {ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal} from '@angular/core';
import {FileSystemFileEntry, NgxFileDropEntry} from 'ngx-file-drop';
import {Observable} from 'rxjs';
import {ToastrService} from 'ngx-toastr';
import {ImageService} from 'src/app/_services/image.service';
import {UploadService} from 'src/app/_services/upload.service';
import {ImageComponent} from "../../shared/image/image.component";
import {translate, TranslocoModule} from "@jsverse/transloco";
import {ColorscapeService} from "../../_services/colorscape.service";
import {
  FileDragAndDropUploadComponent
} from "src/app/shared/file-drag-and-drop-upload/file-drag-and-drop-upload.component";
import {NgbNav, NgbNavContent, NgbNavItem, NgbNavLink, NgbNavOutlet} from "@ng-bootstrap/ng-bootstrap";
import {TabTitlePipe} from "../../_pipes/tab-title.pipe";
import {Tabs} from "../../_models/tabs";

export interface CoverImageOption {
  url: string;
  title: string;
  subtitle?: string;
}

export interface ICoverImageChooserConfig {
  isLocked?: boolean | null;
  resetFunc?: () => Observable<unknown>;
  selected?: CoverImageOption;
  volumeFunc?: Observable<CoverImageOption[]>;
  chapterFunc?: Observable<CoverImageOption[]>;
  kavitaplusFunc?: Observable<CoverImageOption[]>;
}

@Component({
  selector: 'app-cover-image-chooser',
  imports: [
    ImageComponent,
    TranslocoModule,
    FileDragAndDropUploadComponent,
    NgbNav,
    NgbNavItem,
    NgbNavLink,
    NgbNavContent,
    NgbNavOutlet,
    TabTitlePipe
  ],
  templateUrl: './cover-image-chooser.component.html',
  styleUrls: ['./cover-image-chooser.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CoverImageChooserComponent {

  public readonly imageService = inject(ImageService);
  private readonly toastr = inject(ToastrService);
  private readonly uploadService = inject(UploadService);
  private readonly colorscapeService = inject(ColorscapeService);

  config = input<ICoverImageChooserConfig>({});

  coverChanged = output<{ isDirty: boolean; url: string }>();
  resetClicked = output();

  protected readonly acceptableExtensions = ['.png', '.jpg', '.jpeg', '.gif', '.webp', '.avif'].join(',');

  protected readonly volumeImages = signal<CoverImageOption[] | null>(null);
  protected readonly chapterImages = signal<CoverImageOption[] | null>(null);
  protected readonly uploadedImages = signal<CoverImageOption[]>([]);
  protected readonly kavitaplusImages = signal<CoverImageOption[]>([]);
  protected readonly selectedOptionKey = signal<string | null>(null);

  private volumeLoaded = false;
  private chapterLoaded = false;
  private kavitaPlusLoaded = false;
  private hasInit = false;

  activeTabId = Tabs.Current;

  protected readonly hasTabs = computed(() => {
    const cfg = this.config();
    return !!(cfg.volumeFunc || cfg.chapterFunc || cfg.kavitaplusFunc);
  });

  constructor() {
    effect(() => {
      // Keep track of the default tab
      const hasUploadedImages = this.uploadedImages().length > 0;
      const hasSelected = this.selectedOptionKey() != null;
      const hasVolume = (this.volumeImages() ?? []).length > 0;
      const hasChapter = (this.chapterImages() ?? []).length > 0;

      if (this.hasInit) return;

      if (hasSelected) {
        this.activeTabId = Tabs.Current;
      } else if (hasUploadedImages) {
        this.activeTabId = Tabs.Uploaded;
      } else if (hasVolume) {
        this.activeTabId = Tabs.Volumes;
      } else if (hasChapter) {
        this.activeTabId = Tabs.Chapters;
      }

      this.hasInit = true;
    });
  }

  selectOption(option: CoverImageOption, sourceTab: Tabs) {
    this.selectedOptionKey.set(option.url);
    const isDirty = sourceTab !== Tabs.Current;

    if (option.url.startsWith('data:image/')) {
      this.coverChanged.emit({ isDirty, url: option.url });
      return;
    }

    const img = new Image();
    img.crossOrigin = 'Anonymous';
    img.src = option.url;
    img.onload = () => {
      const base64 = this.colorscapeService.getBase64Image(img);
      this.coverChanged.emit({ isDirty, url: base64 });
    };
    img.onerror = () => {
      this.toastr.error(translate('errors.rejected-cover-upload'));
    };
  }

  onTabActivate(tabId: Tabs) {
    if (tabId === Tabs.Volumes && !this.volumeLoaded && this.config().volumeFunc) {
      this.volumeLoaded = true;
      this.config().volumeFunc!.subscribe(items => {this.volumeImages.set(items)});
    }
    if (tabId === Tabs.Chapters && !this.chapterLoaded && this.config().chapterFunc) {
      this.chapterLoaded = true;
      this.config().chapterFunc!.subscribe(items => {this.chapterImages.set(items)});
    }
    if (tabId === Tabs.KavitaPlus && !this.kavitaPlusLoaded && this.config().kavitaplusFunc) {
      this.chapterLoaded = true;
      this.config().kavitaplusFunc!.subscribe(items => {this.kavitaplusImages.set(items)});
    }
  }

  reset() {
    const fn = this.config().resetFunc;
    if (fn) {
      fn().subscribe(() => {
        this.resetClicked.emit(undefined);
        this.selectedOptionKey.set(null);
      });
    } else {
      this.resetClicked.emit(undefined);
      this.selectedOptionKey.set(null);
    }
  }

  public dropped(files: NgxFileDropEntry[]) {
    for (const droppedFile of files) {
      if (droppedFile.fileEntry.isFile) {
        const fileEntry = droppedFile.fileEntry as FileSystemFileEntry;
        fileEntry.file((file: File) => {
          const reader = new FileReader();
          reader.onload = (e) => {
            if (e.target?.result) {
              this.addToUploaded(e.target.result as string);
            }
          };
          reader.readAsDataURL(file);
        });
      }
    }
  }

  loadImage(url: string) {
    if (!url || url === '') return;

    this.uploadService.uploadByUrl(url).subscribe(filename => {
      const img = new Image();
      img.crossOrigin = 'Anonymous';
      img.src = this.imageService.getCoverUploadImage(filename);
      img.onload = () => {
        const base64 = this.colorscapeService.getBase64Image(img);
        this.addToUploaded(base64);
      };
      img.onerror = () => {
        this.toastr.error(translate('errors.rejected-cover-upload'));
      };
    });
  }

  private addToUploaded(base64: string) {
    const option: CoverImageOption = { url: base64, title: '' };
    this.uploadedImages.update(imgs => [...imgs, option]);
    this.selectOption(option, Tabs.Uploaded);
  }

  addImage(option: CoverImageOption) {
    this.uploadedImages.update(imgs => [...imgs, option]);
    this.selectOption(option, Tabs.Uploaded);
  }

  protected readonly Tabs = Tabs;
}
