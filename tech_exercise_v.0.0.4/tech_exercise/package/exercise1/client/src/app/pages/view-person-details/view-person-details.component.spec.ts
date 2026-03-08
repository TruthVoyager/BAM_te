import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ActivatedRoute } from '@angular/router';
import { ViewPersonDetailsComponent } from './view-person-details.component';

describe('ViewPersonDetailsComponent', () => {
  let component: ViewPersonDetailsComponent;
  let fixture: ComponentFixture<ViewPersonDetailsComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewPersonDetailsComponent, HttpClientTestingModule],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'TestPerson' } } } },
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ViewPersonDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    const req = httpMock.expectOne((r) => r.url.includes('/AstronautDuty/'));
    req.flush({ success: true, person: { name: 'TestPerson' }, astronautDuties: [] });
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should request duties by name and set person and duties on success', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ViewPersonDetailsComponent, HttpClientTestingModule],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'TestPerson' } } } },
      ],
    }).compileComponents();
    const ctrl = TestBed.inject(HttpTestingController);
    const f = TestBed.createComponent(ViewPersonDetailsComponent) as ComponentFixture<ViewPersonDetailsComponent>;
    f.detectChanges();
    const req = ctrl.expectOne((r: any) => r.url.includes('/AstronautDuty/'));
    const duties = [
      { id: 1, dutyTitle: 'Current', dutyEndDate: null, rankName: 'Captain', rankId: 1, rankLevel: 3, personId: 1, dutyStartDate: '2024-01-01' },
      { id: 2, dutyTitle: 'Past', dutyEndDate: '2023-12-31', rankName: 'Sergeant', rankId: 2, rankLevel: 2, personId: 1, dutyStartDate: '2023-01-01' },
    ];
    req.flush({ success: true, person: { name: 'TestPerson' }, astronautDuties: duties });
    expect(f.componentInstance.loading).toBe(false);
    expect(f.componentInstance.person?.name).toBe('TestPerson');
    expect(f.componentInstance.currentDuty?.dutyTitle).toBe('Current');
    expect(f.componentInstance.pastDuties.length).toBe(1);
    expect(f.componentInstance.pastDuties[0].dutyTitle).toBe('Past');
    ctrl.verify();
  });

  it('should set error when name param is missing', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ViewPersonDetailsComponent, HttpClientTestingModule],
      providers: [{ provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } }],
    }).compileComponents();
    const f = TestBed.createComponent(ViewPersonDetailsComponent) as ComponentFixture<ViewPersonDetailsComponent>;
    f.detectChanges();
    expect(f.componentInstance.error).toBe('No person specified');
    expect(f.componentInstance.loading).toBe(false);
    TestBed.inject(HttpTestingController).verify();
  });

  it('should set error on API failure', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ViewPersonDetailsComponent, HttpClientTestingModule],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'TestPerson' } } } },
      ],
    }).compileComponents();
    const ctrl = TestBed.inject(HttpTestingController);
    const f = TestBed.createComponent(ViewPersonDetailsComponent) as ComponentFixture<ViewPersonDetailsComponent>;
    f.detectChanges();
    const req = ctrl.expectOne((r: any) => r.url.includes('/AstronautDuty/'));
    req.flush({ success: false, message: 'Not found' }, { status: 404, statusText: 'Not Found' });
    expect(f.componentInstance.loading).toBe(false);
    expect(f.componentInstance.error).toBeTruthy();
    ctrl.verify();
  });

  it('should set error when success is false or person missing', () => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ViewPersonDetailsComponent, HttpClientTestingModule],
      providers: [
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'TestPerson' } } } },
      ],
    }).compileComponents();
    const ctrl = TestBed.inject(HttpTestingController);
    const f = TestBed.createComponent(ViewPersonDetailsComponent) as ComponentFixture<ViewPersonDetailsComponent>;
    f.detectChanges();
    const req = ctrl.expectOne((r: any) => r.url.includes('/AstronautDuty/'));
    req.flush({ success: false, message: 'Not found' });
    expect(f.componentInstance.error).toBe('Not found');
    ctrl.verify();
  });
});
