import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

// IMPORTURI COMPONENTE – fișierele VS
import { Login } from './pages/login/login';
import { Dashboard } from './pages/dashboard/dashboard';
import { Vehicles } from './pages/vehicles/vehicles';
import { VehicleForm } from './pages/vehicles/vehicle-form/vehicle-form';
import { Customers } from './pages/customers/customers';
import { CustomerForm } from './pages/customers/customer-form/customer-form';
import { Rentals } from './pages/rentals/rentals';
import { RentalForm } from './pages/rentals/rental-form/rental-form';
import { Reports } from './pages/reports/reports';
import { Staff } from './pages/admin/staff/staff';
import { Branches } from './pages/admin/branches/branches';

@NgModule({
  declarations: [
    App,
    Login,
    Dashboard,
    Vehicles,
    VehicleForm,
    Customers,
    CustomerForm,
    Rentals,
    RentalForm,
    Reports,
    Staff,
    Branches
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
