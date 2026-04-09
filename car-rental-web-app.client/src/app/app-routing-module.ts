import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// IMPORTURI COMPONENTE – folosit fișierele generate de Visual Studio
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

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: Login },
  { path: 'dashboard', component: Dashboard },

  { path: 'vehicles', component: Vehicles },
  { path: 'vehicles/add', component: VehicleForm },
  { path: 'vehicles/edit/:id', component: VehicleForm },

  { path: 'customers', component: Customers },
  { path: 'customers/add', component: CustomerForm },
  { path: 'customers/edit/:id', component: CustomerForm },

  { path: 'rentals', component: Rentals },
  { path: 'rentals/create', component: RentalForm },

  { path: 'reports', component: Reports },

  { path: 'admin/staff', component: Staff },
  { path: 'admin/branches', component: Branches },

  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
