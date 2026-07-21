# Handoff: LPG Distributor ERP — Management System UX

## Overview
A back-office web application for an LPG (cooking-gas) cylinder distributor in Bangladesh. It covers the full distribution workflow: dashboard, reporting, daily marketing-vehicle operations, sales/purchase transactions, payments, cylinder & gas ledgers, driver/salesman settlements, customer credit, and master data (brands, products, warehouses, suppliers, etc.). Currency is Bangladeshi Taka (৳). All data shown is illustrative sample data.

## About the Design Files
The files in this bundle are **design references created in HTML** — interactive prototypes showing the intended look, layout, and behavior. They are **not production code to copy directly**. They are authored as "Design Components" (a lightweight in-house HTML+JS runtime); the markup uses custom tags like `<x-dc>`, `<sc-for>`, `<sc-if>`, and `{{ }}` holes that will **not** run in a normal app.

The task is to **recreate these designs in the target codebase's environment** (React, Vue, etc.) using its established component library, routing, and state patterns. If no frontend environment exists yet, React + a utility CSS approach (or the team's preferred stack) is a reasonable choice. Treat the HTML as the source of truth for visual spec and interaction intent; re-implement the logic idiomatically.

## Fidelity
**High-fidelity.** Final colors, typography, spacing, and interaction patterns are specified. Recreate the UI pixel-accurately using the codebase's existing primitives. Exact tokens are listed under **Design Tokens** below.

## Global Shell (shared by every screen)
Every page is a full-height two-pane layout:

- **Sidebar** — fixed `width: 232px`, background `#14161d`, non-scrolling header/footer with a scrollable nav body.
  - Brand block: 32×32 rounded (`8px`) logo tile with gradient `linear-gradient(135deg, #f97316, #dc2626)`, letter "L" (white, 800). Title "LPG ERP" (14px/700, `#fff`), subtitle "DISTRIBUTOR SUITE" (10px, `#6b7280`, uppercase, letter-spacing 0.08em).
  - Nav grouped under uppercase labels (10px/700, `#5b6270`, letter-spacing 0.12em): **Overview, Operations, Transactions, Settlements, Customers, Master Data**.
  - Nav item: flex row, `gap: 10px`, `padding: 7px 10px`, `border-radius: 7px`, 13px/500. Idle color `#9aa1ad`; hover `background: rgba(255,255,255,0.06); color:#fff`; **active** `color:#fff; background: rgba(249,115,22,0.16)`. Leading 18px glyph icon (unicode symbols, `opacity: 0.85`).
  - Footer: 30px round avatar "SR" on `#374151`, name "Subrata Roy" (12px/600), role "Admin" (11px, `#6b7280`). Top border `1px solid rgba(255,255,255,0.07)`.
- **Main** — flex column, `min-width: 0`.
  - **Top bar** — `height: 56px`, `background:#fff`, bottom border `#e7e9ee`, `padding: 0 24px`. Left: breadcrumb (`Group / Page`, 13px; muted `#9ca3af` with active segment `#1c1f26/600`). Right: a 260px search pill (`#f4f5f7` bg, `#e7e9ee` border, 8px radius) with a `⌘K` mono chip, and a 34px bell button with a red dot badge.
  - **Content** — scroll area, `padding: 24px 28px 40px`. Standard page header row: `<h1>` 22px/800, letter-spacing -0.01em, plus a subtitle (13px, `#6b7280`) on the left and action buttons on the right.

**Nav → file map** (used verbatim by every page; hrefs are the filenames):
`Dashboard, Reports, Vehicle Loadings, Vehicle Closings, Salesmen (Salesmen Page.dc.html), Drivers, Trucks, Routes, Sales Orders, Purchase Orders, Payments, Stock Transfers, Daily Sales, Driver Settlements, Salesman Settlements, Cylinder Deposits, Cylinder Exchanges, Customers, Cylinder Ledger, Gas Ledger, Credit, Advance Refills, Notifications, Brands, Warehouses, Products, Cylinders, Cylinder Sizes, Suppliers, Transport Companies`.

## Screens / Views

### Dashboard (`Dashboard.dc.html`)
- **Purpose**: At-a-glance daily operating picture.
- **Layout**: Header with a segmented range toggle (Today / This week / This month). Then: a 4-up KPI grid; a 2-col row (`1.8fr / 1fr`) of a 14-day stacked bar chart (Cash `#ea580c` + Credit `#fdba74`) and a "Vehicles on route" list with progress bars; a 2-col row of a "Low stock alerts" table and a "Cylinder stock by brand" panel with stacked filled/empty bars.
- **KPI card**: `#fff`, `1px solid #e7e9ee`, radius 12px, `padding 16px 18px`. Label 12px/600 `#6b7280`; 26px rounded icon chip (tinted bg/fg pairs); value 24px/800; colored delta (green `#15803d` up, red `#dc2626` down); sub 11px `#9ca3af`.

### Reports (`Reports.dc.html`)
- **Purpose**: Business analytics for a period.
- **Layout**: Header with range toggle (This week / This month / This quarter) + "↓ Export PDF". Then: a 4-up "report library" of link cards (icon chip + name + sub, hover `border-color:#ea580c; background:#fffaf6`); a 4-up KPI strip; a 2-col row (`1.8fr/1fr`) of a grouped "Revenue vs. collection" bar chart (Revenue `#ea580c`, Collected `#cbd5e1`) and a "Payment mix" panel (single stacked bar + legend rows); a 2-col row (`1.4fr/1fr`) of a "Top products by revenue" table (rank, name, units, revenue, share bar) and a "Salesman leaderboard" (avatar, name, orders/route, sales, delta); a full-width "Receivables aging" panel (4 buckets: Current/1–30/31–60/60+, each a mini card with amount, customer count, progress bar).
- **Note**: report-library `<a>` cards and their inner text wrapper both need `min-width: 0` so ellipsis engages (prevents horizontal overflow at 4-up).

### Vehicle Loadings (`Vehicle Loadings.dc.html`)
- **Purpose**: Today's marketing vehicles and their stock/sell-through.
- **Layout**: Header with "◷ History" (links to `Loading History.dc.html`) and "+ New Loading" (opens the New Loading drawer, below). Body is a responsive card grid `repeat(auto-fill, minmax(340px, 1fr))`, gap 16px.
- **Vehicle card**: rounded 14px. Top: 42px icon tile, plate (15px/800 mono), status pill (Selling green / Loading amber / Returned blue, each `bg`+`color`+matching dot), route + departure. Two crew chips (salesman/driver, initials avatars). Per-item progress rows (name, sold/loaded, bar colored green if >70% sold else `#ea580c`). Footer strip (`#fafbfc`): Cash (green), Credit (amber), Empties, and a contextual action button.
- **New Loading drawer** (slide-in, right, `width: 500px`): overlay `rgba(15,17,23,0.4)`; header (title + "Draft LD-1143" + ✕). Body: **Assignment** 2-col grid of selects (Vehicle, Route, Salesman, Driver, Loading date, Departure time); **Load items** list of rows `[product select | qty number(74px) | remove ✕(34px)]` with an "+ Add" button that appends a row; a totals card (`#f8f9fb`) showing line count + `Σ cylinders`; an amber notice about stock reservation. Footer: Cancel + "Confirm loading". State: `drawerOpen`, `rows[{id, qty}]`, `nextId`; add/remove/setQty mutate rows; total = Σ qty.

### Loading History (`Loading History.dc.html`)
- **Purpose**: Historical list of every vehicle loading with close-out summary.
- **Layout**: Header with "↓ Export" + "+ New Loading" (returns to Vehicle Loadings). A 4-up summary-stat strip. Then a card containing: a toolbar (search input, status filter chips All/Selling/Closed/Cancelled, date-range select, result count) and a horizontally-scrollable table (`min-width: 980px`) with columns **Vehicle, Date, Salesman/Route, Loaded, Sold, Returned, Cash, Status, (action)**. Status pill colors: Loading amber, Selling green, Closed blue, Cancelled red. Pagination footer.

### Sales Orders (`Sales Orders.dc.html`)
- **Purpose**: Browse/create sales orders (packages, refills, cylinders, accessories).
- **Layout**: Header "↓ Export" + "+ New Order". A 4-card **status pipeline** (Draft / Confirmed / Partially paid / Delivered) — each is a toggle filter (selected card gets `#fff7ed` bg + `#ea580c` border). Then a table card: toolbar (search, warehouse select, date select, count) + a scrollable table (`min-width: 940px`) columns **Order #, Customer, Items, Payment, Amount, Due, Status, (actions)**. Row click opens a view drawer; row buttons → view, ✎ edit.
- **Drawer** (right, `width: 440px`): view mode = label/value list; edit/new mode = form fields (Customer, Date, Items, Payment select, Amount, Due, Status select). Footer switches: view → Close + "✎ Edit"; form → Cancel + "Save Order". New order auto-numbers `SO-<2419+rev>` and prepends to the list.
- Payment badge colors: Cash green, Credit amber, Mobile blue, Bank purple. Status badge: Draft grey, Confirmed blue, Partial amber, Delivered green.

### Purchase Orders / Payments / Stock Transfers / Daily Sales (and most Master Data / Settlements / Customer pages)
- **Purpose**: List + CRUD for each entity.
- **Layout**: Standard page header with a secondary button + primary "+ New …". A config-driven table card: toolbar (search input + result count) + table whose columns are declared in a `cfg` object per page. Each row supports click-to-view, ✎ edit, and delete (confirm).
- **Drawer** (right, `width: 420px`): identical structure to Sales Orders' drawer but generated from `cfg.fields`. Modes: `view` (read-only label/value rows), `edit`, `new`. `save()` builds/patches a record and prepends new ones.
- **Cell renderer kinds** (`cfg.cols[].kind`): `mono` (JetBrains Mono ref codes, `#6b7280`), `main` (primary text `#1c1f26/600`, optional `sub`), `badge` (pill from a `{value:[bg,fg]}` map), `money` (`৳ ` prefix, 700), `num`, `muted` (12px `#6b7280`). Badge fallback `['#f4f5f7','#6b7280']`.

> Because these pages share one engine, implement **one reusable `EntityListPage` + `EntityDrawer`** component pair driven by a per-entity config, rather than 25 bespoke pages. Dashboard, Reports, Vehicle Loadings, and Loading History are the bespoke exceptions.

## Interactions & Behavior
- **Navigation**: plain page-to-page links via the sidebar (each nav item routes to its entity page). In a SPA, map these to routes.
- **Range/status toggles**: local state; selecting re-styles the active control and (in a real app) refilters data. Pipeline/status chips toggle off when re-clicked (back to "All").
- **Search**: case-insensitive substring match across the row's values (Sales Orders matches order # and customer specifically).
- **Drawers**: open from "+ New …", row click (view), or ✎ (edit). Backdrop click and ✕ close. Edit buttons on rows call `stopPropagation` so they don't also trigger the row's view handler.
- **CRUD**: create prepends to the in-memory list; edit mutates the current record; delete removes after `confirm()`. All illustrative/in-memory — wire to real APIs.
- **Hover states**: buttons darken (`#ea580c → #c2410c`), light buttons/rows go to `#f4f5f7`/`#fafbfc`, nav items lighten. Inputs on focus: `border-color:#ea580c; box-shadow: 0 0 0 3px rgba(234,88,12,0.12)`.
- No page transitions/animations beyond the drawer slide-in and hover color changes. Charts are static CSS bars (no chart lib required, but a lib is fine).

## State Management
Per entity list page: `query` (search), an optional filter (`status`/`range`), and drawer state `mode: 'view'|'edit'|'new'|null`, `current` (selected record), `form` (working copy), and a `rev` counter to force refresh after in-memory mutation. Vehicle Loadings' New Loading drawer holds `drawerOpen` + editable `rows`. Replace in-memory arrays with real data fetching/caching in the target stack.

## Design Tokens
**Colors**
- App background `#f4f5f7`; surfaces `#fff`; primary text `#1c1f26`; secondary `#374151`; muted `#6b7280`; faint `#9ca3af`.
- Borders: `#e7e9ee` (card), `#eef0f3` / `#f1f3f6` (inner rows), `#e0e3e9` (inputs/secondary buttons). Subtle fills `#fafbfc`, `#f8f9fb`.
- Sidebar: `#14161d`; sidebar text `#e5e7eb` / `#9aa1ad`; sidebar labels `#5b6270`; dividers `rgba(255,255,255,0.07)`.
- **Primary/accent (orange)**: `#ea580c` (buttons/bars), hover `#c2410c`, deep `#9a3412`; tints `#fff7ed`, `#ffedd5`, `#fed7aa`, `#fdba74`; link color `#c2410c`.
- **Semantic pills** (bg / fg): green `#f0fdf4`/`#15803d`; amber `#fefce8`/`#a16207` (also `#d97706`); blue `#eff6ff`/`#1d4ed8`; purple `#faf5ff`/`#7e22ce`; red `#fef2f2`/`#dc2626`; grey `#f4f5f7`/`#6b7280`; teal accent `#0f766e`.
- Brand chart hues: Bashundhara `#ea580c`, Omera `#1d4ed8`, BM `#0f766e`, Petromax `#7e22ce`.
- Gradient (logo, KPI accents): `linear-gradient(135deg, #f97316, #dc2626)`.

**Typography**
- Body/UI: **Public Sans** (weights 400/500/600/700/800), `-webkit-font-smoothing: antialiased`.
- Mono (codes, plates, ⌘K): **JetBrains Mono** (500/600).
- Scale: h1 22px/800 (-0.01em); section title 14px/700; KPI value 22–24px/800 (-0.02em); body 13px; secondary 12px; captions/labels 10–11px (uppercase labels 700, letter-spacing 0.05–0.12em).

**Radius**: cards 12–14px; buttons/inputs 7–8px; pills/badges 20px; icon tiles 7–10px; avatars 50%.

**Spacing**: content padding `24px 28px 40px`; card padding `16–20px`; grid gaps 14–16px; button padding `9px 14–18px`; input padding `10px 12px`.

**Shadows**: primary button `0 1px 2px rgba(234,88,12,0.35)`; drawer `-12px 0 40px rgba(15,17,23,0.15)`; drawer overlay `rgba(15,17,23,0.4)`.

## Assets
None required. All icons are Unicode glyphs (e.g. `◫ ▤ ⛟ ⇄ ৳ ⚖ ◉ ◈`) and the bell/search are emoji/glyphs — replace with the codebase's icon set (map each glyph to the nearest icon). Fonts load from Google Fonts (Public Sans, JetBrains Mono). No raster images or logos.

## Files
Design reference files (all `.dc.html`, in the project root):
- `Dashboard.dc.html`, `Reports.dc.html`
- `Vehicle Loadings.dc.html`, `Loading History.dc.html`, `Vehicle Closings.dc.html`, `Salesmen Page.dc.html`, `Drivers.dc.html`, `Trucks.dc.html`, `Routes.dc.html`
- `Sales Orders.dc.html`, `Purchase Orders.dc.html`, `Payments.dc.html`, `Stock Transfers.dc.html`, `Daily Sales.dc.html`
- `Driver Settlements.dc.html`, `Salesman Settlements.dc.html`, `Cylinder Deposits.dc.html`, `Cylinder Exchanges.dc.html`
- `Customers.dc.html`, `Cylinder Ledger.dc.html`, `Gas Ledger.dc.html`, `Credit.dc.html`, `Advance Refills.dc.html`, `Notifications.dc.html`
- `Brands.dc.html`, `Warehouses.dc.html`, `Products.dc.html`, `Cylinders.dc.html`, `Cylinder Sizes.dc.html`, `Suppliers.dc.html`, `Transport Companies.dc.html`

Copies of every design file are included alongside this README. `support.js` is the prototype runtime — reference only; do not port it.
