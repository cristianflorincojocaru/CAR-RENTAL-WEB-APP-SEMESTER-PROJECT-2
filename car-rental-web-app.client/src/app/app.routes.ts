import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent }    from './pages/home/home';
import { AboutComponent }   from './pages/about us/about';
import { Contact }          from './pages/contact/contact';
import { CarsComponent }    from './pages/cars/cars';
import { OffersComponent }  from './pages/offers/offers';
import { LoginComponent }   from './pages/auth/login/login';
import { SignupComponent }  from './pages/auth/signup/signup';
import { BookingComponent } from './pages/booking/booking';

import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  // ── Rute publice ─────────────────────────────────────────────
  { path: '',        component: HomeComponent },
  { path: 'about',   component: AboutComponent },
  { path: 'contact', component: Contact },
  { path: 'offers',  component: OffersComponent },
  { path: 'cars',    component: CarsComponent },

  // ── Rute de autentificare ─────────────────────────────────────
  { path: 'login',  component: LoginComponent },
  { path: 'signup', component: SignupComponent },

  // ── Rute protejate (necesită autentificare) ───────────────────
  {
    path: 'cars/:id/book',
    component: BookingComponent,
    canActivate: [authGuard]
  },

  // ── Fallback ──────────────────────────────────────────────────
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
