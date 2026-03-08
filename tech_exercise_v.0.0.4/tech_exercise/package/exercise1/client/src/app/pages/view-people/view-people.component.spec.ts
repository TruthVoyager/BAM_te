import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute } from '@angular/router';
import { ViewPeopleComponent } from './view-people.component';

describe('ViewPeopleComponent', () => {
  let component: ViewPeopleComponent;
  let fixture: ComponentFixture<ViewPeopleComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewPeopleComponent, HttpClientTestingModule],
      providers: [{ provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } }],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ViewPeopleComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && !r.url.includes('/Person/'));
    req.flush({ success: true, people: [] });
  });

  it('should show loading initially then people on success', () => {
    expect(component.loading).toBe(true);
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && !r.url.includes('/Person/'));
    req.flush({ success: true, people: [{ personId: 1, name: 'Alice' }] });
    expect(component.loading).toBe(false);
    expect(component.people.length).toBe(1);
    expect(component.people[0].name).toBe('Alice');
    expect(component.error).toBeNull();
  });

  it('should set error when API returns success false', () => {
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && !r.url.includes('/Person/'));
    req.flush({ success: false, message: 'Server error' });
    expect(component.loading).toBe(false);
    expect(component.error).toBe('Server error');
  });

  it('should set error on HTTP error', () => {
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && !r.url.includes('/Person/'));
    req.flush('error', { status: 500, statusText: 'Server Error' });
    expect(component.loading).toBe(false);
    expect(component.error).toBeTruthy();
  });

  it('should set error from res.message when success but no people array', () => {
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && !r.url.includes('/Person/'));
    req.flush({ success: false, message: 'Custom error' });
    expect(component.error).toBe('Custom error');
  });
});
