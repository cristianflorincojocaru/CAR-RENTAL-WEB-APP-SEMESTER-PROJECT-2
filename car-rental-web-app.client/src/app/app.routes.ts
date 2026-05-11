import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

<<<<<<< HEAD
import { HomeComponent }      from './pages/home/home';
import { AboutComponent }     from './pages/about us/about';
import { Contact }            from './pages/contact/contact';
import { CarsComponent }      from './pages/cars/cars';
import { OffersComponent }    from './pages/offers/offers';
import { LoginComponent }     from './pages/auth/login/login';
import { SignupComponent }    from './pages/auth/signup/signup';
import { BookingComponent }   from './pages/booking/booking';
import { DashboardComponent } from './pages/dashboard/dashboard';
=======
import { HomeComponent }        from './pages/home/home';
import { AboutComponent }       from './pages/about us/about';
import { Contact }              from './pages/contact/contact';
import { CarsComponent }        from './pages/cars/cars';
import { OffersComponent }      from './pages/offers/offers';
import { LoginComponent }       from './pages/auth/login/login';
import { SignupComponent }      from './pages/auth/signup/signup';
import { BookingComponent }     from './pages/booking/booking';
import { MyBookingsComponent }  from './pages/my-bookings/my-bookings';
>>>>>>> 08c83621ab69d66b6c9aa022cfa1f52ec8204519

import { authGuard, roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  // ── Rute publice ─────────────────────────────────────────────
  { path: '',        component: HomeComponent    },
  { path: 'about',   component: AboutComponent  },
  { path: 'contact', component: Contact         },
  { path: 'offers',  component: OffersComponent },
  { path: 'cars',    component: CarsComponent   },

  // ── Rute de autentificare ─────────────────────────────────────
  { path: 'login',  component: LoginComponent,  data: { animation: 'login'  } },
  { path: 'signup', component: SignupComponent, data: { animation: 'signup' } },
<<<<<<< HEAD

  // ── Dashboard — accesibil doar pentru Administrator, Manager, Operator
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard],
    data: { animation: 'dashboard' }
  },
=======
>>>>>>> 08c83621ab69d66b6c9aa022cfa1f52ec8204519

  // ── Rute protejate (necesită autentificare) ───────────────────
  {
    path: 'cars/:id/book',
    component: BookingComponent,
    canActivate: [authGuard]
  },
  {
    path: 'my-bookings',
    component: MyBookingsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'profile',
    redirectTo: '',
    pathMatch: 'full'
  },

  // ── Fallback ──────────────────────────────────────────────────
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}