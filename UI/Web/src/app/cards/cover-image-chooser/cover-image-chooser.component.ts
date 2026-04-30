import {ChangeDetectionStrategy, Component, computed, inject, input, output, signal} from '@angular/core';
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

export interface CoverImageOption {
  url: string;
  title: string;
  subtitle?: string;
}

export interface ICoverImageChooserConfig {
  showReset?: boolean;
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
    NgbNavOutlet
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

  acceptableExtensions = ['.png', '.jpg', '.jpeg', '.gif', '.webp', '.avif'].join(',');

  protected readonly volumeImages = signal<CoverImageOption[] | null>(null);
  protected readonly chapterImages = signal<CoverImageOption[] | null>(null);
  protected readonly uploadedImages = signal<CoverImageOption[]>([]);
  protected readonly selectedOptionKey = signal<string | null>(null);

  private volumeLoaded = false;
  private chapterLoaded = false;

  protected readonly hasTabs = computed(() => {
    const cfg = this.config();
    return !!(cfg.volumeFunc || cfg.chapterFunc || cfg.kavitaplusFunc);
  });

  selectOption(option: CoverImageOption, sourceTab: 'upload' | 'current' | 'volume' | 'chapter' | 'kavitaplus') {
    this.selectedOptionKey.set(option.url);
    const isDirty = sourceTab !== 'current';

    if (option.url.startsWith('data:image/')) {
      this.coverChanged.emit({ isDirty, url: option.url });
      return;
    }

    const img = new Image();
    img.crossOrigin = 'Anonymous';
    img.src = option.url;
    img.onload = () => {
      const base64 = this.colorscapeService.getBase64Image(img);
      this.selectedOptionKey.set(base64);
      this.coverChanged.emit({ isDirty, url: base64 });
    };
    img.onerror = () => {
      this.toastr.error(translate('errors.rejected-cover-upload'));
    };
  }

  onTabActivate(tabId: string) {
    if (tabId === 'volume' && !this.volumeLoaded && this.config().volumeFunc) {
      this.volumeLoaded = true;
      this.config().volumeFunc!.subscribe(items => this.volumeImages.set(items));
    }
    if (tabId === 'chapter' && !this.chapterLoaded && this.config().chapterFunc) {
      this.chapterLoaded = true;
      this.config().chapterFunc!.subscribe(items => this.chapterImages.set(items));
    }
  }

  reset() {
    this.resetClicked.emit(undefined);
    this.selectedOptionKey.set(null);
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
    this.selectOption(option, 'upload');
  }

  addImage(option: CoverImageOption) {
    this.uploadedImages.update(imgs => [...imgs, option]);
    this.selectOption(option, 'upload');
  }
}
