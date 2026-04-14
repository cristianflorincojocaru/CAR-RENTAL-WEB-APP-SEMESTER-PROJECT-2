import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface Stat {
  value: string;
  label: string;
}

interface Value {
  icon: string;
  title: string;
  description: string;
}

interface TeamMember {
  initials: string;
  avatarBg: string;
  name: string;
  role: string;
  bio: string;
}

interface Milestone {
  year: string;
  title: string;
  description: string;
}

interface Perk {
  icon: string;
  title: string;
  description: string;
}

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './about.html',
  styleUrls: ['./about.scss']
})
export class AboutComponent {

  // --- Hero stats strip ---
  stats: Stat[] = [
    { value: '2018',  label: 'founded'         },
    { value: '100+',  label: 'vehicles'         },
    { value: '15k+',  label: 'happy customers'  },
    { value: '4.9★',  label: 'average rating'   }
  ];

  // --- Mission values ---
  values: Value[] = [
    {
      icon: `<svg width="22" height="22" viewBox="0 0 22 22" fill="none">
               <circle cx="11" cy="11" r="9" stroke="#1A56DB" stroke-width="1.6"/>
               <path d="M7 11l3 3 5-5" stroke="#1A56DB" stroke-width="1.6"
                     stroke-linecap="round" stroke-linejoin="round"/>
             </svg>`,
      title: 'Full transparency',
      description: 'The price you see is the price you pay — no add-ons at the counter.'
    },
    {
      icon: `<svg width="22" height="22" viewBox="0 0 22 22" fill="none">
               <path d="M11 2l2 7h7l-5.5 4 2 7L11 16l-5.5 4 2-7L2 9h7z"
                     stroke="#1A56DB" stroke-width="1.6" stroke-linejoin="round"/>
             </svg>`,
      title: 'Quality fleet',
      description: 'Every car is less than 3 years old, serviced regularly and spotlessly clean.'
    },
    {
      icon: `<svg width="22" height="22" viewBox="0 0 22 22" fill="none">
               <circle cx="11" cy="8" r="4" stroke="#1A56DB" stroke-width="1.6"/>
               <path d="M3 20a8 8 0 0116 0" stroke="#1A56DB" stroke-width="1.6"
                     stroke-linecap="round"/>
             </svg>`,
      title: 'Human support',
      description: 'Real people, available 24/7 by phone or chat — not just a chatbot.'
    }
  ];

  // --- Team ---
  team: TeamMember[] = [
    {
      initials: 'AM',
      avatarBg: 'linear-gradient(135deg, #1A56DB, #3B82F6)',
      name: 'Alexandru Marin',
      role: 'Co-founder & CEO',
      bio: 'Former logistics manager with a passion for making mobility simpler. Alex started DriveNow after one too many frustrating rental experiences.'
    },
    {
      initials: 'IR',
      avatarBg: 'linear-gradient(135deg, #0F172A, #1A56DB)',
      name: 'Ioana Radu',
      role: 'Head of Operations',
      bio: 'Ioana keeps the fleet running like clockwork — from scheduling maintenance to ensuring every car is ready before you even arrive.'
    },
    {
      initials: 'MN',
      avatarBg: 'linear-gradient(135deg, #1D4ED8, #60A5FA)',
      name: 'Mihai Neagu',
      role: 'Customer Experience Lead',
      bio: 'If you have ever called our support line, there is a good chance you spoke to Mihai. He believes no rental issue is too small to fix — fast.'
    }
  ];

  // --- Milestones ---
  milestones: Milestone[] = [
    {
      year: '2018',
      title: 'The beginning',
      description: 'Launched with just 8 cars and a big idea: rent a car in under 3 minutes, delivered to your door in Bucharest.'
    },
    {
      year: '2019',
      title: 'First 1,000 customers',
      description: 'Word spread fast. We hit 1,000 happy rentals and expanded our fleet to 30 vehicles, adding our first SUV models.'
    },
    {
      year: '2021',
      title: 'National expansion',
      description: 'Opened pickup points in Cluj-Napoca, Timișoara and Iași — making DriveNow available across all major Romanian cities.'
    },
    {
      year: '2023',
      title: 'Going green',
      description: 'Introduced our first hybrid and electric vehicles, reducing fleet emissions by 20% as part of our sustainability pledge.'
    },
    {
      year: '2024',
      title: '15,000 customers & counting',
      description: 'Reached a milestone we are proud of: 15,000 verified customers and a 4.9-star rating across all review platforms.'
    }
  ];

  // --- Perks ---
  perks: Perk[] = [
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <path d="M12 28L14 20Q16 14 20 12Q24 14 26 20L28 28"
                     stroke="#1A56DB" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
               <circle cx="16" cy="28" r="2.5" fill="#1A56DB"/>
               <circle cx="24" cy="28" r="2.5" fill="#1A56DB"/>
               <path d="M13 23h14" stroke="#1A56DB" stroke-width="1.5" stroke-linecap="round"/>
             </svg>`,
      title: 'Modern fleet',
      description: 'All vehicles are 2022 or newer, fully equipped and rigorously inspected before every rental.'
    },
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <rect x="11" y="13" width="18" height="15" rx="3" stroke="#1A56DB" stroke-width="1.8"/>
               <path d="M15 10v4M25 10v4M11 19h18" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round"/>
             </svg>`,
      title: 'Instant booking',
      description: 'Complete your reservation in under 2 minutes — no phone calls, no waiting, instant email confirmation.'
    },
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <path d="M14 20a6 6 0 100-12 6 6 0 000 12z" stroke="#1A56DB" stroke-width="1.8" fill="none"/>
               <path d="M10 38a10 10 0 0120 0" stroke="#1A56DB" stroke-width="1.8" stroke-linecap="round"/>
               <path d="M24 22l4 4M28 22l-4 4" stroke="#1A56DB" stroke-width="1.8" stroke-linecap="round"/>
             </svg>`,
      title: '24/7 support',
      description: 'Our team is always reachable — by phone, chat or email — whenever you need help on the road.'
    },
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <path d="M20 12v8l5 3" stroke="#1A56DB" stroke-width="1.8" stroke-linecap="round"/>
               <circle cx="20" cy="20" r="8" stroke="#1A56DB" stroke-width="1.8"/>
             </svg>`,
      title: 'Flexible returns',
      description: 'Short term, long term, or somewhere in between — our rental periods fit your schedule, not the other way around.'
    },
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <path d="M13 20l5 5 9-9" stroke="#1A56DB" stroke-width="2"
                     stroke-linecap="round" stroke-linejoin="round"/>
             </svg>`,
      title: 'No hidden fees',
      description: 'The price shown at search is exactly what you pay. Full insurance included, no unpleasant surprises at the counter.'
    },
    {
      icon: `<svg width="40" height="40" viewBox="0 0 40 40" fill="none">
               <circle cx="20" cy="20" r="18" fill="#EFF6FF"/>
               <path d="M20 10c-3 0-6 2-8 5l8 5 8-5c-2-3-5-5-8-5z"
                     stroke="#1A56DB" stroke-width="1.8" stroke-linejoin="round" fill="none"/>
               <path d="M12 15l8 5v10" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round" stroke-linejoin="round"/>
               <path d="M28 15l-8 5" stroke="#1A56DB" stroke-width="1.8"
                     stroke-linecap="round"/>
             </svg>`,
      title: 'Hotel & airport delivery',
      description: 'We bring the car to you. Pick any hotel, airport or city-centre address as your collection point.'
    }
  ];
}
