import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { HttpErrorResponse } from '@angular/common/http';

import { ContactService } from '../../services/contact.service';
import { ContactMessageRequest } from '../../models/contact.models';

interface Branch {
  id: number;
  name: string;
  county: string;
  address: string;
  phone: string;
  hoursDisplay: string;
  fleet: string;
  mapSrc: SafeResourceUrl;
  openHour: number;
  openMinute: number;
  closeHour: number;
  closeMinute: number;
  weekdaysOnly: boolean;
}

interface ContactForm {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
  message: string;
}

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss',
})
export class Contact implements OnInit, OnDestroy {

  // ── State formular ────────────────────────────────────────────
  formSent        = false;
  formError       = '';
  formCooldown    = false;
  cooldownSeconds = 0;
  isSubmitting    = false;
  private cooldownInterval: ReturnType<typeof setInterval> | null = null;

  form: ContactForm = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    subject: '',
    message: '',
  };

  // ── Branch-uri ────────────────────────────────────────────────
  activeBranch = 0;
  branches: Branch[];

  constructor(
    private sanitizer: DomSanitizer,
    private contactService: ContactService
  ) {
    this.branches = [
      {
        id: 0,
        name: 'Bucharest — Central',
        county: 'Bucharest, Romania',
        address: 'Șos. Kiseleff 1, București 011341',
        phone: '+40 21 201 4000',
        hoursDisplay: 'Mon – Fri: 09:00 – 17:00',
        fleet: 'Fleet: 40 vehicles available',
        mapSrc: this.safe(
          'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2848.5!2d26.0793!3d44.4532!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x40b202c0ddca51c7%3A0x5d0d6e7e7c7c7c7c!2sPalatul+Kiseleff!5e1!3m2!1sro!2sro!4v1700000010000'
        ),
        openHour: 9, openMinute: 0, closeHour: 17, closeMinute: 0,
        weekdaysOnly: true,
      },
      {
        id: 1,
        name: 'Bucharest — Otopeni',
        county: 'Ilfov County, Romania',
        address: 'Calea București-Ploiești, Otopeni 075150',
        phone: '+40 21 204 1000',
        hoursDisplay: 'Daily: 07:00 – 23:30',
        fleet: 'Fleet: 35 vehicles available',
        mapSrc: this.safe(
          'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d11512.6!2d26.0850!3d44.5722!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x40b202065b66b56d%3A0x5a7a52d5d4e4c0a0!2sHenri+Coanda+International+Airport!5e1!3m2!1sro!2sro!4v1700000000002'
        ),
        openHour: 7, openMinute: 0, closeHour: 23, closeMinute: 30,
        weekdaysOnly: false,
      },
      {
        id: 2,
        name: 'Cluj — Airport',
        county: 'Cluj County, Romania',
        address: 'Str. Traian Vuia 149, Cluj-Napoca 400397',
        phone: '+40 264 416 702',
        hoursDisplay: 'Daily: 07:00 – 23:30',
        fleet: 'Fleet: 28 vehicles available',
        mapSrc: this.safe(
          'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d11512.6!2d23.6862!3d46.7852!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x47490e7c4e4b4b4b%3A0x1c2e3d4f5a6b7c8d!2sCluj+International+Airport!5e1!3m2!1sro!2sro!4v1700000000005'
        ),
        openHour: 7, openMinute: 0, closeHour: 23, closeMinute: 30,
        weekdaysOnly: false,
      },
      {
        id: 3,
        name: 'Timișoara — Airport',
        county: 'Timiș County, Romania',
        address: 'Str. Aeroport 2, Ghiroda 307200',
        phone: '+40 256 386 000',
        hoursDisplay: 'Daily: 07:00 – 23:30',
        fleet: 'Fleet: 17 vehicles available',
        mapSrc: this.safe(
          'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d11512.6!2d21.3379!3d45.8099!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x474567ca7e5e4a7b%3A0x3d0a2b5c4e6f7a8b!2sTimișoara+International+Airport!5e1!3m2!1sro!2sro!4v1700000000006'
        ),
        openHour: 7, openMinute: 0, closeHour: 23, closeMinute: 30,
        weekdaysOnly: false,
      },
    ];
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    if (this.cooldownInterval) clearInterval(this.cooldownInterval);
  }

  get activeBranchData(): Branch {
    return this.branches[this.activeBranch];
  }

  selectBranch(index: number): void {
    this.activeBranch = index;
  }

  /** Scroll smooth to top of page — used by footer Contact Us link */
  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  getOpenStatus(branch: Branch): 'Open now' | 'Closed' {
    const now   = new Date();
    const roStr = now.toLocaleString('en-US', { timeZone: 'Europe/Bucharest' });
    const ro    = new Date(roStr);
    const day   = ro.getDay();
    const mins  = ro.getHours() * 60 + ro.getMinutes();
    const openMins  = branch.openHour  * 60 + branch.openMinute;
    const closeMins = branch.closeHour * 60 + branch.closeMinute;

    if (branch.weekdaysOnly && (day === 0 || day === 6)) return 'Closed';
    return mins >= openMins && mins < closeMins ? 'Open now' : 'Closed';
  }

  isOpen(branch: Branch): boolean {
    return this.getOpenStatus(branch) === 'Open now';
  }

  // ── Submit formular → API ─────────────────────────────────────

  submitForm(): void {
    if (this.formCooldown || this.isSubmitting) return;

    if (!this.form.firstName || !this.form.email || !this.form.message) {
      this.formError = 'Please fill in your name, email and message.';
      return;
    }

    this.isSubmitting = true;
    this.formError    = '';

    const request: ContactMessageRequest = {
      firstName: this.form.firstName.trim(),
      lastName:  this.form.lastName.trim(),
      email:     this.form.email.trim(),
      phone:     this.form.phone?.trim() || undefined,
      subject:   this.form.subject  || 'General inquiry',
      message:   this.form.message.trim(),
    };

    this.contactService.sendMessage(request).subscribe({
      next: () => {
        this.isSubmitting    = false;
        this.formSent        = true;
        this.formCooldown    = true;
        this.cooldownSeconds = 10;
        this.startCooldown();
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        if (err.status === 429) {
          this.formError = 'Too many messages sent. Please wait a moment before trying again.';
        } else if (err.status === 0) {
          this.formError = 'Cannot connect to server. Please try again later.';
        } else {
          this.formError = 'Failed to send message. Please try again.';
        }
      }
    });
  }

  private startCooldown(): void {
    this.cooldownInterval = setInterval(() => {
      this.cooldownSeconds--;
      if (this.cooldownSeconds <= 0) {
        clearInterval(this.cooldownInterval!);
        this.cooldownInterval = null;
        this.formSent         = false;
        this.formCooldown     = false;
        this.form = {
          firstName: '', lastName: '', email: '',
          phone: '', subject: '', message: '',
        };
      }
    }, 1000);
  }

  private safe(url: string): SafeResourceUrl {
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }
}
