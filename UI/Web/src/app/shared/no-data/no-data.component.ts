import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoDirective } from '@jsverse/transloco';

export interface LibraryNoDataConfig {
  title?: string;
  subtitle?: string;
  message?: string;
  showBookshelf?: boolean;
  bookshelfSize?: 'small' | 'medium' | 'large';
  variant?: 'default' | 'search' | 'filter' | 'empty-library' | 'maintenance';
  iconOnly?: boolean;
  customIcon?: string;
  modernStyle?: 'bookshelf' | 'digital' | 'cloud' | 'cards' | 'minimal';
}

@Component({
  selector: 'app-no-data',
  standalone: true,
  imports: [CommonModule, TranslocoDirective],
  templateUrl: './no-data.component.html',
  styleUrls: ['./no-data.component.scss']
})
export class NoDataComponent {
  // Basic text customization
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() message?: string;
  
  // Theme and appearance
  @Input() isDarkTheme: boolean = false;
  @Input() showBookshelf: boolean = true;
  @Input() bookshelfSize: 'small' | 'medium' | 'large' = 'medium';
  @Input() iconOnly: boolean = false;
  @Input() customIcon?: string;
  
  // Modern style options
  @Input() modernStyle: 'bookshelf' | 'digital' | 'cloud' | 'cards' | 'minimal' = 'bookshelf';
  
  // Predefined variants for different scenarios
  @Input() variant: 'default' | 'search' | 'filter' | 'empty-library' | 'maintenance' = 'default';
  
  // Configuration object (alternative to individual inputs)
  @Input() config?: LibraryNoDataConfig;
  
  // Advanced customization
  @Input() showEmojis: boolean = true;
  @Input() customClass?: string;
  @Input() minHeight?: number;

  // Data for digital grid
  digitalItems = [
    { icon: '📄' }, { icon: '📊' }, { icon: '📈' },
    { icon: '📋' }, { icon: '🗂️' }, { icon: '📁' },
    { icon: '📄' }, { icon: '📑' }, { icon: '🔍' }
  ];

  // Data for cloud points
  cloudPoints = Array.from({ length: 6 }, (_, i) => ({ id: i }));

  // Computed properties based on variant and config
  get effectiveTitle(): string {
    if (this.config?.title || this.title) {
      return this.config?.title || this.title || '';
    }
    
    switch (this.variant) {
      case 'search':
        return 'No Results Found';
      case 'filter':
        return 'No Items Match Your Filters';
      case 'empty-library':
        return 'Welcome to Your Library';
      case 'maintenance':
        return 'Library Under Maintenance';
      default:
        return 'Your Library Awaits';
    }
  }

  get effectiveSubtitle(): string {
    if (this.config?.subtitle || this.subtitle) {
      return this.config?.subtitle || this.subtitle || '';
    }
    
    switch (this.variant) {
      case 'search':
        return 'Try adjusting your search terms';
      case 'filter':
        return 'Try adjusting your filters to see more results';
      case 'empty-library':
        return 'Start building your collection';
      case 'maintenance':
        return 'We\'ll be back shortly';
      default:
        return 'The shelves are ready for new stories';
    }
  }

  get effectiveMessage(): string {
    if (this.config?.message || this.message) {
      return this.config?.message || this.message || '';
    }
    
    switch (this.variant) {
      case 'search':
        return 'We couldn\'t find any series matching your search. Try different keywords or browse by genre.';
      case 'filter':
        return 'No series match your current filter criteria. Try removing some filters or expanding your selection.';
      case 'empty-library':
        return 'This library is ready for its first series. Add some books to begin your reading journey.';
      case 'maintenance':
        return 'The library is temporarily unavailable while we make improvements. Please check back soon.';
      default:
        return 'This collection is currently empty. Try adjusting your filters or adding new series to begin your reading journey.';
    }
  }

  get effectiveIcon(): string {
    if (this.customIcon) {
      return this.customIcon;
    }
    
    switch (this.variant) {
      case 'search':
        return '🔍';
      case 'filter':
        return '🔧';
      case 'empty-library':
        return '🏛️';
      case 'maintenance':
        return '🔧';
      default:
        return '📚';
    }
  }

  get shouldShowBookshelf(): boolean {
    if (this.config?.showBookshelf !== undefined) {
      return this.config.showBookshelf;
    }
    return this.showBookshelf && !this.iconOnly && this.variant !== 'maintenance';
  }

  get effectiveBookshelfSize(): 'small' | 'medium' | 'large' {
    return this.config?.bookshelfSize || this.bookshelfSize;
  }

  get containerClasses(): string {
    const classes = ['no-data-container'];
    
    if (this.isDarkTheme) {
      classes.push('dark-theme');
    }
    
    if (this.customClass) {
      classes.push(this.customClass);
    }
    
    classes.push(`variant-${this.variant}`);
    classes.push(`size-${this.effectiveBookshelfSize}`);
    classes.push(`style-${this.modernStyle}`);
    
    if (this.iconOnly) {
      classes.push('icon-only');
    }
    
    return classes.join(' ');
  }

  get containerStyles(): { [key: string]: string } {
    const styles: { [key: string]: string } = {};
    
    if (this.minHeight) {
      styles['min-height'] = `${this.minHeight}px`;
    }
    
    return styles;
  }
}