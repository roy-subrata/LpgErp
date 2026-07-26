import { Component, EventEmitter, Input, Output, signal, HostListener, ElementRef, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface DropdownMenuItem {
  label: string;
  icon?: string;
  danger?: boolean;
  disabled?: boolean;
}

@Component({
  selector: 'app-dropdown-menu',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dropdown-wrapper" (click)="$event.stopPropagation()">
      <button #trigger class="kebab-btn" (click)="toggle()" [class.open]="open()">
        <span class="kebab-icon">⋮</span>
      </button>
      @if (open()) {
        <div class="dropdown-panel" [style.top.px]="panelTop()" [style.left.px]="panelLeft()">
          @for (item of items; track $index) {
            <button
              class="dropdown-item"
              [class.danger]="item.danger"
              [class.disabled]="item.disabled"
              [disabled]="item.disabled"
              (click)="onSelect($index); $event.stopPropagation()">
              @if (item.icon) {
                <span class="item-icon">{{ item.icon }}</span>
              }
              <span>{{ item.label }}</span>
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: inline-block; }

    .dropdown-wrapper {
      position: relative;
      display: inline-flex;
    }

    .kebab-btn {
      width: 30px;
      height: 30px;
      border: 1px solid var(--border);
      border-radius: 6px;
      background: var(--surface);
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      transition: background 0.15s, border-color 0.15s;
    }
    .kebab-btn:hover,
    .kebab-btn.open {
      background: var(--fill-subtle);
      border-color: var(--text-muted);
    }

    .kebab-icon {
      font-size: 16px;
      font-weight: 700;
      color: var(--text-secondary);
      line-height: 1;
    }

    .dropdown-panel {
      position: fixed;
      min-width: 150px;
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: 8px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.12), 0 2px 8px rgba(0,0,0,0.06);
      z-index: 9999;
      padding: 4px;
      animation: dropdownFadeIn 0.12s ease-out;
    }

    @keyframes dropdownFadeIn {
      from { opacity: 0; transform: translateY(-4px); }
      to { opacity: 1; transform: translateY(0); }
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 8px;
      width: 100%;
      padding: 8px 12px;
      border: none;
      border-radius: 5px;
      background: transparent;
      font-size: 13px;
      font-weight: 500;
      color: var(--text-primary);
      cursor: pointer;
      text-align: left;
      transition: background 0.1s;
      white-space: nowrap;
    }
    .dropdown-item:hover {
      background: var(--fill-subtle);
    }
    .dropdown-item.danger {
      color: var(--red-fg);
    }
    .dropdown-item.danger:hover {
      background: var(--red-bg);
    }
    .dropdown-item.disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }
    .dropdown-item.disabled:hover {
      background: transparent;
    }

    .item-icon {
      font-size: 14px;
      width: 18px;
      text-align: center;
      flex-shrink: 0;
    }
  `],
})
export class DropdownMenuComponent {
  @Input() items: DropdownMenuItem[] = [];
  @Output() selected = new EventEmitter<number>();
  @ViewChild('trigger') triggerRef!: ElementRef<HTMLButtonElement>;

  open = signal(false);
  panelTop = signal(0);
  panelLeft = signal(0);

  constructor(private eRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.open.set(false);
    }
  }

  toggle() {
    if (!this.open()) {
      this.positionPanel();
    }
    this.open.update(v => !v);
  }

  private positionPanel() {
    const btn = this.triggerRef?.nativeElement;
    if (!btn) return;
    const rect = btn.getBoundingClientRect();
    const menuHeight = this.items.length * 36 + 8;
    const spaceBelow = window.innerHeight - rect.bottom;
    const openAbove = spaceBelow < menuHeight + 8;
    this.panelTop.set(openAbove ? rect.top - menuHeight - 4 : rect.bottom + 4);
    this.panelLeft.set(Math.min(rect.left, window.innerWidth - 170));
  }

  onSelect(index: number) {
    if (this.items[index]?.disabled) return;
    this.selected.emit(index);
    this.open.set(false);
  }
}
