import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoDirective } from '@jsverse/transloco';

export interface NoDataConfig {
  title?: string;
  subtitle?: string;
  message?: string;
  variant?: 'default' | 'search' | 'filter' | 'empty-library' | 'maintenance';
  customIcon?: string;
  size?: 'small' | 'medium' | 'large';
}

@Component({
  selector: 'app-no-data',
  standalone: true,
  imports: [CommonModule, TranslocoDirective],
  templateUrl: './no-data.component.html',
  styleUrls: ['./no-data.component.scss']
})
export class NoDataComponent {
  // Basic customization inputs
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() message?: string;
  @Input() variant: 'default' | 'search' | 'filter' | 'empty-library' | 'maintenance' = 'default';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() isDarkTheme: boolean = false;
  @Input() customClass?: string;
  @Input() minHeight?: number;

  // Configuration object (alternative to individual inputs)
  @Input() config?: NoDataConfig;

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
        return 'Your Library Awaits';
      case 'maintenance':
        return 'Under Maintenance';
      default:
        return 'Nothing Here Yet';
    }
  }

  get effectiveSubtitle(): string {
    if (this.config?.subtitle || this.subtitle) {
      return this.config?.subtitle || this.subtitle || '';
    }
    
    switch (this.variant) {
      case 'search':
        return 'Try different search terms or browse categories';
      case 'filter':
        return 'Adjust your filters to discover more content';
      case 'empty-library':
        return 'Ready to be filled with amazing stories and knowledge';
      case 'maintenance':
        return 'We\'ll be back shortly with improvements';
      default:
        return 'This space is ready for your content';
    }
  }

  get effectiveMessage(): string {
    if (this.config?.message || this.message) {
      return this.config?.message || this.message || '';
    }
    
    switch (this.variant) {
      case 'search':
        return 'Refine your search or explore different categories to find what you\'re looking for.';
      case 'filter':
        return 'Try removing some filters or broadening your criteria to see more results.';
      case 'empty-library':
        return 'Start your collection by adding books, series, or documents to this library.';
      case 'maintenance':
        return 'Our team is working hard to improve your experience. Thank you for your patience.';
      default:
        return 'Add your first item to get started and bring this space to life.';
    }
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
    classes.push(`size-${this.config?.size || this.size}`);
    
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