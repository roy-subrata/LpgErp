import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (open) {
      <div class="dialog-overlay" (click)="close.emit()">
        <div class="dialog" (click)="$event.stopPropagation()">
          <div class="dialog-header">
            <h2>{{ title }}</h2>
            <button class="dialog-close" (click)="close.emit()">&times;</button>
          </div>
          <div class="dialog-body">
            <ng-content />
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .dialog-overlay {
      position: fixed; top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center;
      z-index: 1000;
    }
    .dialog {
      background: white; border-radius: 8px; width: 90%; max-width: 600px;
      max-height: 90vh; overflow-y: auto; box-shadow: 0 4px 20px rgba(0,0,0,0.3);
    }
    .dialog-header {
      display: flex; justify-content: space-between; align-items: center;
      padding: 1rem 1.5rem; border-bottom: 1px solid #eee;
    }
    .dialog-header h2 { margin: 0; font-size: 1.2rem; }
    .dialog-close {
      background: none; border: none; font-size: 1.5rem; cursor: pointer;
      color: #666; padding: 0 0.25rem;
    }
    .dialog-close:hover { color: #333; }
    .dialog-body { padding: 1.5rem; }
  `],
})
export class DialogComponent {
  @Input() open = false;
  @Input() title = '';
  @Output() close = new EventEmitter<void>();
}
