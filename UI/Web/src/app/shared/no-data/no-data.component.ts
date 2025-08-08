import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoDirective } from '@jsverse/transloco';

export interface NoDataConfig {
  title?: string;
  subtitle?: string;
  message?: string;
  variant?: NoDataVariant;
  size?: NoDataSize;
  showActionButton?: boolean;
  actionButtonText?: string;
  actionButtonIcon?: string;
  customIcon?: string;
  animationType?: AnimationType;
  theme?: 'light' | 'dark' | 'auto';
  backgroundTheme?: 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror' | 'random' | 'none';
}

export type NoDataVariant = 
  | 'empty-library' 
  | 'search-no-results' 
  | 'filter-no-results' 
  | 'maintenance' 
  | 'error' 
  | 'loading' 
  | 'welcome' 
  | 'achievement' 
  | 'custom';

export type NoDataSize = 'small' | 'medium' | 'large' | 'fullscreen';

export type AnimationType = 'floating' | 'pulse' | 'wave' | 'bounce' | 'fade' | 'none';

@Component({
  selector: 'app-no-data',
  standalone: true,
  imports: [CommonModule, TranslocoDirective],
  templateUrl: './no-data.component.html',
  styleUrls: ['./no-data.component.scss']
})
export class NoDataComponent implements OnInit {
  // Basic inputs
  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() message?: string;
  @Input() variant: NoDataVariant = 'empty-library';
  @Input() size: NoDataSize = 'medium';
  @Input() animationType: AnimationType = 'floating';
  @Input() theme: 'light' | 'dark' | 'auto' = 'auto';
  @Input() showActionButton: boolean = false;
  @Input() actionButtonText?: string;
  @Input() actionButtonIcon?: string;
  @Input() customIcon?: string;
  @Input() customClass?: string;
  @Input() minHeight?: number;
  @Input() backgroundTheme?: 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror' | 'random' | 'none';

  // Output for action button click
  @Output() actionButtonClick = new EventEmitter<void>();

  // Background theme randomization
  private currentBackgroundTheme: 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror' | 'none' = 'fantasy';

  // Available background themes for randomization
  private readonly availableBackgroundThemes: Array<'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror'> = [
    'fantasy',
    'scifi', 
    'comic-book',
    'manga',
    'thriller',
    'mystery',
    'romance',
    'adventure',
    'horror'
  ];

  // Configuration object (alternative to individual inputs)
  @Input() config?: NoDataConfig;

  // Computed properties
  get effectiveConfig(): NoDataConfig {
    return {
      title: this.config?.title || this.title,
      subtitle: this.config?.subtitle || this.subtitle,
      message: this.config?.message || this.message,
      variant: this.config?.variant || this.variant,
      size: this.config?.size || this.size,
      showActionButton: this.config?.showActionButton ?? this.showActionButton,
      actionButtonText: this.config?.actionButtonText || this.actionButtonText,
      actionButtonIcon: this.config?.actionButtonIcon || this.actionButtonIcon,
      customIcon: this.config?.customIcon || this.customIcon,
      animationType: this.config?.animationType || this.animationType,
      theme: this.config?.theme || this.theme,
      backgroundTheme: this.config?.backgroundTheme || this.backgroundTheme || 'random'
    };
  }

  get effectiveTitle(): string {
    if (this.effectiveConfig.title !== undefined && this.effectiveConfig.title !== '') {
      return this.effectiveConfig.title;
    }
    
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'No Results Found';
      case 'filter-no-results':
        return 'No Items Match Your Filters';
      case 'empty-library':
        return 'Your Library Awaits';
      case 'maintenance':
        return 'Under Maintenance';
      case 'error':
        return 'Something Went Wrong';
      case 'loading':
        return 'Loading...';
      case 'welcome':
        return 'Welcome to Your Library';
      case 'achievement':
        return 'Achievement Unlocked!';
      default:
        return 'Nothing Here Yet';
    }
  }

  get shouldShowTitle(): boolean {
    return this.effectiveConfig.title !== undefined && this.effectiveConfig.title !== '';
  }

  get effectiveSubtitle(): string {
    if (this.effectiveConfig.subtitle !== undefined && this.effectiveConfig.subtitle !== '') {
      return this.effectiveConfig.subtitle;
    }
    
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'Try different search terms or browse categories';
      case 'filter-no-results':
        return 'Adjust your filters to discover more content';
      case 'empty-library':
        return 'Ready to be filled with amazing stories and knowledge';
      case 'maintenance':
        return 'We\'ll be back shortly with improvements';
      case 'error':
        return 'Don\'t worry, we\'re here to help';
      case 'loading':
        return 'Please wait while we prepare your content';
      case 'welcome':
        return 'Start your journey of discovery today';
      case 'achievement':
        return 'You\'ve reached a milestone!';
      default:
        return 'This space is ready for your content';
    }
  }

  get shouldShowSubtitle(): boolean {
    return this.effectiveConfig.subtitle !== undefined && this.effectiveConfig.subtitle !== '';
  }

  get effectiveMessage(): string {
    if (this.effectiveConfig.message !== undefined && this.effectiveConfig.message !== '') {
      return this.effectiveConfig.message;
    }
    
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'Refine your search or explore different categories to find what you\'re looking for.';
      case 'filter-no-results':
        return 'Try removing some filters or broadening your criteria to see more results.';
      case 'empty-library':
        return 'Start your collection by adding books, series, or documents to this library.';
      case 'maintenance':
        return 'Our team is working hard to improve your experience. Thank you for your patience.';
      case 'error':
        return 'We encountered an unexpected issue. Please try refreshing the page or contact support if the problem persists.';
      case 'loading':
        return 'We\'re gathering your content and preparing everything for you.';
      case 'welcome':
        return 'Add your first item to get started and bring this space to life.';
      case 'achievement':
        return 'Congratulations on reaching this milestone! Keep exploring to discover more.';
      default:
        return 'Add your first item to get started and bring this space to life.';
    }
  }

  get shouldShowMessage(): boolean {
    return this.effectiveConfig.message !== undefined && this.effectiveConfig.message !== '';
  }

  get effectiveActionButtonText(): string {
    if (this.effectiveConfig.actionButtonText) {
      return this.effectiveConfig.actionButtonText;
    }
    
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'Browse Categories';
      case 'filter-no-results':
        return 'Clear Filters';
      case 'empty-library':
        return 'Add Content';
      case 'error':
        return 'Try Again';
      case 'welcome':
        return 'Get Started';
      default:
        return 'Take Action';
    }
  }

  get effectiveActionButtonIcon(): string {
    if (this.effectiveConfig.actionButtonIcon) {
      return this.effectiveConfig.actionButtonIcon;
    }
    
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'fas fa-search';
      case 'filter-no-results':
        return 'fas fa-filter';
      case 'empty-library':
        return 'fas fa-plus';
      case 'error':
        return 'fas fa-redo';
      case 'welcome':
        return 'fas fa-rocket';
      default:
        return 'fas fa-arrow-right';
    }
  }

  get containerClasses(): string {
    const classes = ['no-data-container'];
    
    classes.push(`variant-${this.effectiveConfig.variant}`);
    classes.push(`size-${this.effectiveConfig.size}`);
    classes.push(`animation-${this.effectiveConfig.animationType}`);
    classes.push(`theme-${this.effectiveConfig.theme}`);
    
    if (this.customClass) {
      classes.push(this.customClass);
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

  get iconClasses(): string {
    const classes = ['no-data-icon'];
    
    if (this.effectiveConfig.customIcon) {
      classes.push(this.effectiveConfig.customIcon);
    } else {
      classes.push(this.getDefaultIcon());
    }
    
    return classes.join(' ');
  }

  get getCurrentBackgroundTheme(): 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror' | 'none' {
    return this.currentBackgroundTheme;
  }

  ngOnInit(): void {
    // Handle background theme for empty-library variant
    if (this.effectiveConfig.variant === 'empty-library') {
      const themeInput = this.effectiveConfig.backgroundTheme;
      
      // If backgroundTheme is missing or "none", don't use any background theme
      if (!themeInput || themeInput === 'none') {
        this.currentBackgroundTheme = 'none';
      } else if (themeInput !== 'random') {
        // Use the specified theme
        this.currentBackgroundTheme = themeInput;
      } else {
        // Truly random background theme selection
        this.currentBackgroundTheme = this.getRandomBackgroundTheme();
      }
    }
  }

  /**
   * Get a truly random background theme from available themes
   * @returns A random background theme
   */
  private getRandomBackgroundTheme(): 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror' {
    // Use crypto.getRandomValues for better randomness if available
    let randomIndex: number;
    
    if (typeof crypto !== 'undefined' && crypto.getRandomValues) {
      const array = new Uint32Array(1);
      crypto.getRandomValues(array);
      randomIndex = array[0] % this.availableBackgroundThemes.length;
    } else {
      // Fallback to Math.random() if crypto is not available
      randomIndex = Math.floor(Math.random() * this.availableBackgroundThemes.length);
    }
    
    return this.availableBackgroundThemes[randomIndex];
  }

  /**
   * Add a new background theme to the available themes array
   * This makes the randomization scalable
   * @param theme The new theme to add
   */
  public addBackgroundTheme(theme: 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror'): void {
    if (!this.availableBackgroundThemes.includes(theme)) {
      this.availableBackgroundThemes.push(theme);
    }
  }

  /**
   * Remove a background theme from the available themes array
   * @param theme The theme to remove
   */
  public removeBackgroundTheme(theme: 'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror'): void {
    const index = this.availableBackgroundThemes.indexOf(theme);
    if (index > -1) {
      this.availableBackgroundThemes.splice(index, 1);
    }
  }

  /**
   * Get all available background themes
   * @returns Array of available background themes
   */
  public getAvailableBackgroundThemes(): Array<'fantasy' | 'scifi' | 'comic-book' | 'manga' | 'thriller' | 'mystery' | 'romance' | 'adventure' | 'horror'> {
    return [...this.availableBackgroundThemes];
  }

  /**
   * Force a new random background theme (useful for testing or dynamic changes)
   */
  public randomizeBackgroundTheme(): void {
    if (this.effectiveConfig.variant === 'empty-library' && this.effectiveConfig.backgroundTheme === 'random') {
      this.currentBackgroundTheme = this.getRandomBackgroundTheme();
    }
  }

  private getDefaultIcon(): string {
    switch (this.effectiveConfig.variant) {
      case 'search-no-results':
        return 'fas fa-search';
      case 'filter-no-results':
        return 'fas fa-filter';
      case 'empty-library':
        return 'fas fa-books';
      case 'maintenance':
        return 'fas fa-tools';
      case 'error':
        return 'fas fa-exclamation-triangle';
      case 'loading':
        return 'fas fa-spinner';
      case 'welcome':
        return 'fas fa-star';
      case 'achievement':
        return 'fas fa-trophy';
      default:
        return 'fas fa-inbox';
    }
  }

  onActionButtonClick(): void {
    // Emit event for action button click
    this.actionButtonClick.emit();
  }
}