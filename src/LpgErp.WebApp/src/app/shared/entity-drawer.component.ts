import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DrawerField {
  key: string;
  label: string;
  type?: 'text' | 'number' | 'select' | 'toggle' | 'date' | 'textarea';
  options?: { label: string; value: any }[];
  placeholder?: string;
  mono?: boolean;
  required?: boolean;
}

@Component({
  selector: 'app-entity-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (open) {
      <div class="drawer-overlay" (click)="onClose()"></div>
      <div class="drawer-panel" (click)="$event.stopPropagation()">
        <div class="drawer-header">
          <div>
            <h2 class="drawer-title">{{ title }}</h2>
            @if (subtitle) {
              <span class="drawer-subtitle">{{ subtitle }}</span>
            }
          </div>
          <button class="drawer-close" (click)="onClose()">✕</button>
        </div>

        <div class="drawer-body">
          @if (mode === 'view') {
            @for (field of fields; track field.key) {
              <div class="view-row">
                <div class="view-label">{{ field.label }}</div>
                <div class="view-value" [class.mono]="field.mono">
                  @if (field.type === 'toggle') {
                    <span class="toggle-display" [class.on]="getFormValue(field.key)">
                      {{ getFormValue(field.key) ? 'Active' : 'Inactive' }}
                    </span>
                  } @else if (field.type === 'select') {
                    {{ getDisplayValue(field.key) }}
                  } @else {
                    {{ formatValue(field, getFormValue(field.key)) }}
                  }
                </div>
              </div>
            }
          } @else {
            <form (ngSubmit)="onSave()">
              @for (field of fields; track field.key) {
                <div class="form-group">
                  <label class="form-label">
                    {{ field.label }}
                    @if (field.required) { <span class="required">*</span> }
                  </label>
                  @if (field.type === 'select') {
                    <select class="form-input" [(ngModel)]="formData[field.key]" [name]="field.key" [required]="field.required || false">
                      <option value="">Select...</option>
                      @for (opt of field.options || []; track opt.value) {
                        <option [ngValue]="opt.value">{{ opt.label }}</option>
                      }
                    </select>
                  } @else if (field.type === 'toggle') {
                    <div class="toggle-switch" (click)="toggleField(field.key)">
                      <div class="toggle-track" [class.on]="formData[field.key]">
                        <div class="toggle-thumb"></div>
                      </div>
                    </div>
                  } @else if (field.type === 'textarea') {
                    <textarea class="form-input form-textarea" [(ngModel)]="formData[field.key]" [name]="field.key" [placeholder]="field.placeholder || ''" [required]="field.required || false" rows="3"></textarea>
                  } @else {
                    <input class="form-input" [class.mono]="field.mono" [type]="field.type || 'text'" [(ngModel)]="formData[field.key]" [name]="field.key" [placeholder]="field.placeholder || ''" [required]="field.required || false" />
                  }
                </div>
              }
            </form>
          }
        </div>

        <div class="drawer-footer">
          @if (mode === 'view') {
            <button class="btn-secondary" (click)="onClose()">Close</button>
            <button class="btn-secondary" (click)="mode = 'edit'">✎ Edit</button>
          } @else {
            <button class="btn-secondary" (click)="onClose()">Cancel</button>
            <button class="btn-primary" [disabled]="saving" (click)="onSave()">
              {{ saving ? 'Saving...' : 'Save' }}
            </button>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    .drawer-overlay {
      position: fixed;
      inset: 0;
      background: rgba(15,17,23,0.4);
      z-index: 40;
    }

    .drawer-panel {
      position: fixed;
      top: 0;
      right: 0;
      bottom: 0;
      width: 440px;
      max-width: 100vw;
      background: var(--surface);
      z-index: 50;
      display: flex;
      flex-direction: column;
      box-shadow: var(--shadow-drawer);
      animation: slideIn 0.2s ease-out;
    }

    @keyframes slideIn {
      from { transform: translateX(100%); }
      to { transform: translateX(0); }
    }

    .drawer-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding: 20px 24px 16px;
      border-bottom: 1px solid var(--border);
    }

    .drawer-title {
      font-size: 16px;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
    }

    .drawer-subtitle {
      font-size: 12px;
      font-family: var(--font-mono);
      color: var(--text-muted);
    }

    .drawer-close {
      width: 30px;
      height: 30px;
      border: none;
      background: var(--fill-subtle);
      border-radius: 6px;
      cursor: pointer;
      font-size: 14px;
      color: var(--text-muted);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .drawer-close:hover {
      background: var(--border);
    }

    .drawer-body {
      flex: 1;
      overflow-y: auto;
      padding: 20px 24px;
    }

    /* View mode */
    .view-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 10px 0;
      border-bottom: 1px solid var(--border-row);
    }

    .view-label {
      font-size: 12px;
      font-weight: 600;
      color: var(--text-muted);
    }

    .view-value {
      font-size: 14px;
      font-weight: 500;
      color: var(--text-primary);
    }

    .view-value.mono {
      font-family: var(--font-mono);
      color: var(--text-muted);
    }

    .toggle-display {
      font-size: 12px;
      font-weight: 600;
      padding: 2px 10px;
      border-radius: 10px;
      background: var(--grey-bg);
      color: var(--grey-fg);
    }
    .toggle-display.on {
      background: var(--green-bg);
      color: var(--green-fg);
    }

    /* Form mode */
    .form-group {
      margin-bottom: 16px;
    }

    .form-label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: var(--text-secondary);
      margin-bottom: 5px;
    }

    .required {
      color: var(--red-fg);
    }

    .form-input {
      width: 100%;
      padding: 9px 12px;
      border: 1px solid var(--border-input);
      border-radius: 7px;
      font-size: 13px;
      color: var(--text-primary);
      background: var(--surface);
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
    }

    .form-input:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(234,88,12,0.12);
    }

    .form-input.mono {
      font-family: var(--font-mono);
    }

    .form-textarea {
      resize: vertical;
      min-height: 60px;
    }

    select.form-input {
      cursor: pointer;
    }

    .toggle-switch {
      cursor: pointer;
      display: inline-block;
    }

    .toggle-track {
      width: 40px;
      height: 22px;
      border-radius: 11px;
      background: #d1d5db;
      position: relative;
      transition: background 0.2s;
    }
    .toggle-track.on {
      background: var(--primary);
    }

    .toggle-thumb {
      width: 18px;
      height: 18px;
      border-radius: 50%;
      background: #fff;
      position: absolute;
      top: 2px;
      left: 2px;
      transition: transform 0.2s;
      box-shadow: 0 1px 3px rgba(0,0,0,0.2);
    }
    .toggle-track.on .toggle-thumb {
      transform: translateX(18px);
    }

    /* Footer */
    .drawer-footer {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 24px;
      border-top: 1px solid var(--border);
    }

    .btn-primary {
      padding: 9px 18px;
      border-radius: 7px;
      border: none;
      background: var(--primary);
      color: #fff;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      box-shadow: var(--shadow-btn);
    }
    .btn-primary:hover {
      background: var(--primary-hover);
    }
    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-secondary {
      padding: 9px 18px;
      border-radius: 7px;
      border: 1px solid var(--border-input);
      background: var(--surface);
      color: var(--text-secondary);
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
    }
    .btn-secondary:hover {
      background: var(--fill-subtle);
    }
  `],
})
export class EntityDrawerComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() subtitle = '';
  @Input() mode: 'view' | 'edit' | 'new' = 'new';
  @Input() fields: DrawerField[] = [];
  @Input() entity: any = null;
  @Input() saving = false;

  @Output() closeDrawer = new EventEmitter<void>();
  @Output() saveEntity = new EventEmitter<any>();

  formData: Record<string, any> = {};

  ngOnChanges() {
    if (this.open) {
      if (this.mode === 'edit' && this.entity) {
        this.formData = { ...this.entity };
      } else if (this.mode === 'new') {
        this.formData = {};
        this.fields.forEach(f => {
          this.formData[f.key] = f.type === 'toggle' ? true : '';
        });
      } else if (this.mode === 'view' && this.entity) {
        this.formData = { ...this.entity };
      }
    }
  }

  getFormValue(key: string): any {
    return this.entity?.[key] ?? this.formData[key];
  }

  getDisplayValue(key: string): string {
    const field = this.fields.find(f => f.key === key);
    const val = this.getFormValue(key);
    if (field?.options) {
      const opt = field.options.find(o => o.value === val);
      return opt?.label ?? val ?? '-';
    }
    return val ?? '-';
  }

  formatValue(field: DrawerField, val: any): string {
    if (val === null || val === undefined) return '-';
    if (typeof val === 'number') {
      return val.toLocaleString('en-IN');
    }
    return String(val);
  }

  toggleField(key: string) {
    this.formData[key] = !this.formData[key];
  }

  onClose() {
    this.closeDrawer.emit();
  }

  onSave() {
    this.saveEntity.emit(this.formData);
  }
}
