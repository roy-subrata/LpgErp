# LPG Cylinder Distribution & Inventory Management System

## Overview

The system is designed for an LPG distributor who manages multiple LPG brands (e.g., Bashundhara, Omera, etc.). The business purchases LPG cylinders from different suppliers or directly from LPG companies, stores them in multiple warehouses, and sells them to retail and wholesale customers.

The system must track both the **gas** and the **cylinder** because they are separate business assets.

## Business Entities

- Company/Distributor (Bashundhara, Omera, etc.)
- Supplier
- Warehouse
- Truck
- Driver
- Salesman
- Customer
- Cylinder
- Gas
- Purchase
- Sale
- Stock Transfer
- Commission
- Customer Cylinder Balance

## Business Model

The business operates as an authorized distributor for multiple LPG companies:

- Bashundhara LPG
- Omera LPG
- BM LPG
- Petromax LPG

Each distributor has different purchase prices, cylinder deposit values, commissions, refill prices, and transportation policies.

## Cylinder Types

The system supports multiple cylinder sizes:

- 5 KG
- 12 KG
- 15 KG
- 20 KG
- 30 KG
- 35 KG
- 45 KG

Each size belongs to a specific brand (e.g., Omera 12 KG, Bashundhara 35 KG).

## Products

### 1. Empty Cylinder
Only cylinder (e.g., 12 KG Empty Cylinder)

### 2. Gas Refill
Only gas - customer already owns a cylinder

### 3. New Package
Cylinder + Gas (e.g., 12 KG Cylinder + 12 KG Gas)

### 4. Accessories
- Regulator
- Pipe
- Burner
- Stove

## Purchase Management

Purchases from LPG Company or Third-party Supplier:

- **Empty Cylinder Purchase**: 100 Empty Cylinders
- **Gas Only Purchase**: Gas for refilling inventory
- **Package Purchase**: 100 Filled Cylinders (Cylinder + Filled Gas)

### Purchase Payment
- Cash
- Credit
- Bank
- Mobile Banking

### Supplier Commission
Some companies provide commission (e.g., 50,000 BDT for 1000 cylinders). Commission adjusts against next purchase automatically.

## Transportation Management

### Company Delivery
Company delivers directly to warehouse

### Own Truck
Record: Truck, Driver, Distance, Fuel, Loading cost. Some companies pay per cylinder for self pickup.

### Outsourced Truck
Record: Transport company, Driver, Cost

## Goods Receiving

After truck arrives, warehouse verifies quantity, missing cylinders, damaged cylinders, and short delivery. Inventory increases only after receiving.

## Warehouse Management

Support multiple warehouses (Main, City, Regional). Stock transfer between warehouses required.

## Stock Tracking

Separate stock for:
- Empty Cylinders
- Filled Cylinders
- Gas
- Accessories

## Customer Types

- Retail Customer
- Wholesale Dealer
- Commercial Customer
- Restaurant
- Hotel
- Industrial Customer

## Sales

- **New Package**: Gets Filled Cylinder
- **Gas Refill**: Customer gives empty, receives filled
- **Empty Cylinder**: Purchase only cylinder
- **Accessories**: Regulator, Pipe, etc.

## Cylinder Exchange

Customer exchanges another company's cylinder (e.g., Omera to Bashundhara). Record: Incoming Brand, Outgoing Brand, Exchange Charge.

## Advance Refill

Business gives filled cylinder when customer has no empty. Track Outstanding Empty Cylinder and Customer Liability.

## Customer Cylinder Ledger

Each customer has cylinder balance tracking received filled cylinders, returned empty cylinders, and outstanding balance.

## Customer Gas Ledger

Track gas purchases, cylinder purchases, payments, outstanding balance, and cylinder deposits.

## Customer Credit

Credit Limit, Due Date, Outstanding Amount, Payment History.

## Customer Notifications

- Payment Due
- Possible Refill Time
- Cylinder Return Reminder
- Outstanding Empty Cylinder
- Credit Limit Exceeded

## Cylinder Deposit

Track Deposit Paid, Deposit Returned, and Refund.

## Marketing Sales Vehicle

Daily marketing vehicle leaves warehouse with inventory. Truck receives inventory (e.g., 50 Packages, 30 Refills, 20 Empty Cylinders).

## Mobile Sales

Sales team sells throughout the day: Cash, Credit, Package, Gas Refill, Cylinder Exchange, Accessories.

## Route Sales

Record: Route, Area, Village, Dealer, Customer, Visit Time, GPS (optional).

## Vehicle Closing

End of day reconciliation: Vehicle Started With, Sold, Remaining, Cash Collected, Credit Sales, Outstanding Amount, Cylinder Exchanges, Returned Empty Cylinders, Damage, Leakage, Variance.

## Daily Sales Summary

Each vehicle submits: Total Sales, Cash Sales, Credit Sales, Packages Sold, Refills Sold, Empty Cylinders Sold, Accessories Sold, Payments Collected, Due Created, Cylinder Balance, Outstanding Cylinders, Stock Returned.

## Driver Settlement

Track: Trip, Fuel, Allowance, Loading Cost, Unloading Cost, Trip Income, Company Pickup Incentive.

## Salesman Settlement

Track: Sales, Collection, Commission, Daily Allowance, Bonus.

## Reporting

### Inventory Reports
- Current stock by warehouse
- Stock by LPG brand
- Filled vs. Empty cylinder inventory
- Low stock alerts
- Cylinder movement history

### Purchase Reports
- Purchases by supplier or LPG company
- Credit purchases
- Commission earned and adjusted
- Transportation cost and incentives

### Sales Reports
- Sales by customer/route/vehicle/salesman/driver
- Brand-wise and cylinder-size-wise sales
- Package vs. refill sales

### Customer Reports
- Outstanding payments
- Cylinder balance
- Advance refill list
- Due payment aging
- Refill history

### Vehicle Reports
- Daily loading and unloading
- Vehicle stock reconciliation
- Route performance
- Driver and salesman productivity

### Financial Reports
- Cash flow
- Accounts receivable
- Supplier payable
- Commission balance
- Deposit liability
- Transportation expenses
- Profit and loss analysis
