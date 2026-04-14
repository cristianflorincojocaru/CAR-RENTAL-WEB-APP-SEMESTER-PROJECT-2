import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AboutComponent } from './about';
import { provideRouter } from '@angular/router';

describe('AboutComponent', () => {
  let component: AboutComponent;
  let fixture: ComponentFixture<AboutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AboutComponent],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(AboutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  // =========================
  // DATA TESTS
  // =========================

  it('should have stats defined', () => {
    expect(component.stats).toBeDefined();
    expect(component.stats.length).toBeGreaterThan(0);
  });

  it('should have values defined', () => {
    expect(component.values).toBeDefined();
    expect(component.values.length).toBeGreaterThan(0);
  });

  it('should have team members defined', () => {
    expect(component.team).toBeDefined();
    expect(component.team.length).toBeGreaterThan(0);
  });

  it('should have milestones defined', () => {
    expect(component.milestones).toBeDefined();
    expect(component.milestones.length).toBeGreaterThan(0);
  });

  it('should have perks defined', () => {
    expect(component.perks).toBeDefined();
    expect(component.perks.length).toBeGreaterThan(0);
  });

  // =========================
  // TEMPLATE RENDER TESTS
  // =========================

  it('should render hero title', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const title = compiled.querySelector('.about-hero__title');
    expect(title?.textContent).toContain('Driving freedom');
  });

  it('should render stats in DOM', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const stats = compiled.querySelectorAll('.about-stat');
    expect(stats.length).toBe(component.stats.length);
  });

  it('should render team members', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const teamCards = compiled.querySelectorAll('.team-card');
    expect(teamCards.length).toBe(component.team.length);
  });

  it('should render milestones timeline items', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const items = compiled.querySelectorAll('.timeline__item');
    expect(items.length).toBe(component.milestones.length);
  });

  it('should render perks', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const perks = compiled.querySelectorAll('.perk-card');
    expect(perks.length).toBe(component.perks.length);
  });

  // =========================
  // CONTENT VALIDATION TESTS
  // =========================

  it('each team member should have required fields', () => {
    component.team.forEach(member => {
      expect(member.name).toBeTruthy();
      expect(member.role).toBeTruthy();
      expect(member.bio).toBeTruthy();
      expect(member.initials).toBeTruthy();
    });
  });

  it('each milestone should have year, title and description', () => {
    component.milestones.forEach(m => {
      expect(m.year).toBeTruthy();
      expect(m.title).toBeTruthy();
      expect(m.description).toBeTruthy();
    });
  });

});