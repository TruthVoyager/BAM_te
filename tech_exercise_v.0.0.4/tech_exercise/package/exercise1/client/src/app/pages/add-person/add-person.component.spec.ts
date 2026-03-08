import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Location } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { AddPersonComponent } from './add-person.component';
import { AstronautDuty } from '../../shared/models/astronaut-duty';

describe('AddPersonComponent', () => {
  let component: AddPersonComponent;
  let fixture: ComponentFixture<AddPersonComponent>;
  let httpMock: HttpTestingController;
  let router: Router;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddPersonComponent, HttpClientTestingModule],
      providers: [
        { provide: Location, useValue: { back: jasmine.createSpy('back') } },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { data: {}, paramMap: { get: () => null } },
            data: { pipe: () => ({ subscribe: (fn: (v: unknown) => void) => fn({}) }) },
            paramMap: { pipe: () => ({ subscribe: (fn: (v: unknown) => void) => fn({}) }) },
          },
        },
      ],
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    fixture = TestBed.createComponent(AddPersonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    const rankReq = httpMock.expectOne((r) => r.url.includes('/Rank'));
    rankReq.flush([{ id: 1, name: 'Captain', level: 3 }]);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load ranks and set first rank as default', () => {
    expect(component.loadingRanks).toBe(false);
    expect(component.ranks.length).toBe(1);
    expect(component.rankId).toBe(1);
  });

  it('displayDutyTitle should return RETIRED when state is retire', () => {
    component.dutySectionState = 'retire';
    expect(component.displayDutyTitle).toBe('RETIRED');
  });

  it('displayDutyTitle should return currentDuty title when readonly', () => {
    component.dutySectionState = 'readonly';
    component.currentDuty = { id: 1, personId: 1, rankId: 1, rankLevel: 1, rankName: 'C', dutyTitle: 'Pilot', dutyStartDate: '', dutyEndDate: null };
    expect(component.displayDutyTitle).toBe('Pilot');
  });

  it('isDutyReadonly should be true for readonly and retire', () => {
    component.dutySectionState = 'readonly';
    expect(component.isDutyReadonly).toBe(true);
    component.dutySectionState = 'retire';
    expect(component.isDutyReadonly).toBe(true);
    component.dutySectionState = 'newDuty';
    expect(component.isDutyReadonly).toBe(false);
  });

  it('startNewDuty should set state to newDuty and clear duty fields', () => {
    component.ranks = [{ id: 1, name: 'C', level: 1 }];
    component.rankId = 1;
    component.dutyTitle = 'Old';
    component.startNewDuty();
    expect(component.dutySectionState).toBe('newDuty');
    expect(component.dutyTitle).toBe('');
    expect(component.rankId).toBe(1);
  });

  it('showRetire should set state and title RETIRED', () => {
    component.allDuties = [{ id: 1, rankLevel: 3 } as any];
    component.ranks = [{ id: 1, name: 'C', level: 3 }];
    component.showRetire();
    expect(component.dutySectionState).toBe('retire');
    expect(component.dutyTitle).toBe('RETIRED');
  });

  it('cancel should call location.back', () => {
    component.cancel();
    expect(location.back).toHaveBeenCalled();
  });

  it('submit with empty name should set error when not edit mode', () => {
    component.editMode = false;
    component.name = '   ';
    component.submit();
    expect(component.error).toBe('Name is required');
  });

  it('create should POST person then navigate on success when not astronaut', () => {
    component.name = 'New Person';
    component.isAstronaut = false;
    component.submit();
    const createReq = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'POST');
    createReq.flush({});
    expect(component.submitting).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/view-people']);
  });

  it('create should set error on person API failure', () => {
    component.name = 'New';
    component.submit();
    const createReq = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'POST');
    createReq.flush({ message: 'Conflict' }, { status: 409, statusText: 'Conflict' });
    expect(component.submitting).toBe(false);
    expect(component.error).toBeTruthy();
  });

  it('ngOnDestroy should complete destroy$', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });

  it('displayDutyTitle should return dutyTitle when newDuty state', () => {
    component.dutySectionState = 'newDuty';
    component.dutyTitle = 'Commander';
    expect(component.displayDutyTitle).toBe('Commander');
  });

  it('displayRankId should return highestRankId when retire', () => {
    component.dutySectionState = 'retire';
    component.allDuties = [{ rankLevel: 3 } as AstronautDuty];
    component.ranks = [{ id: 1, name: 'C', level: 3 }];
    expect(component.displayRankId).toBe(1);
  });

  it('displayRankId should return currentDuty.rankId when readonly', () => {
    component.dutySectionState = 'readonly';
    component.currentDuty = { rankId: 2 } as AstronautDuty;
    expect(component.displayRankId).toBe(2);
  });

  it('displayRankName should return rank name when retire and highestRankId set', () => {
    component.dutySectionState = 'retire';
    component.ranks = [{ id: 1, name: 'Colonel', level: 4 }];
    component.allDuties = [{ rankLevel: 4 } as AstronautDuty];
    expect(component.displayRankName).toBe('Colonel');
  });

  it('displayRankName should return currentDuty.rankName when readonly', () => {
    component.dutySectionState = 'readonly';
    component.currentDuty = { rankName: 'Captain' } as AstronautDuty;
    expect(component.displayRankName).toBe('Captain');
  });

  it('displayRankName should return rank name from ranks when newDuty', () => {
    component.dutySectionState = 'newDuty';
    component.rankId = 1;
    component.ranks = [{ id: 1, name: 'Sergeant', level: 2 }];
    expect(component.displayRankName).toBe('Sergeant');
  });

  it('highestRankId should return null when allDuties empty', () => {
    component.allDuties = [];
    expect(component.highestRankId).toBeNull();
  });

  it('resetDutyDisplayToCurrent with no currentDuty should clear duty fields', () => {
    component.currentDuty = null;
    component.ranks = [{ id: 1, name: 'C', level: 1 }];
    component.dutyTitle = 'Old';
    component.rankId = 2;
    component.resetDutyDisplayToCurrent();
    expect(component.dutyTitle).toBe('');
    expect(component.rankId).toBe(1);
  });

  it('cancelDutyEdit should reset duty display to current', () => {
    component.currentDuty = { dutyTitle: 'Pilot', rankId: 1, dutyStartDate: '2024-01-01' } as AstronautDuty;
    component.dutyTitle = 'Other';
    component.cancelDutyEdit();
    expect(component.dutyTitle).toBe('Pilot');
  });

  it('create should require duty title and rank when astronaut', () => {
    component.name = 'Astro';
    component.isAstronaut = true;
    component.dutyTitle = '';
    component.rankId = 1;
    component.submit();
    expect(component.error).toBe('Duty title and rank are required when adding an astronaut');
  });

  it('create as astronaut should POST person then duty then navigate', () => {
    component.name = 'Astro';
    component.isAstronaut = true;
    component.dutyTitle = 'Pilot';
    component.rankId = 1;
    component.dutyStartDate = '2024-01-01';
    component.submit();
    const createReq = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'POST');
    createReq.flush({});
    const dutyReq = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    dutyReq.flush({});
    expect(router.navigate).toHaveBeenCalledWith(['/view-people']);
  });

  it('create as astronaut should set error when duty API fails', () => {
    component.name = 'Astro';
    component.isAstronaut = true;
    component.dutyTitle = 'Pilot';
    component.rankId = 1;
    component.submit();
    const createReq = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'POST');
    createReq.flush({});
    const dutyReq = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    dutyReq.flush({ message: 'Bad request' }, { status: 400, statusText: 'Bad Request' });
    expect(component.error).toBeTruthy();
  });

  it('save should do nothing when editName is missing', () => {
    component.editMode = true;
    component.editName = null;
    component.name = 'X';
    component.submit();
    expect(component.submitting).toBe(false);
  });

  it('save should set error when newDuty and duty title/rank missing', () => {
    component.editMode = true;
    component.editName = 'Original';
    component.name = 'Original';
    component.isAstronaut = true;
    component.dutySectionState = 'newDuty';
    component.dutyTitle = '';
    component.rankId = 1;
    component.submit();
    expect(component.error).toBe('Duty title and rank are required for new duty');
  });

  it('save should update person then navigate when no duty op', () => {
    component.editMode = true;
    component.editName = 'Original';
    component.name = 'Updated';
    component.isAstronaut = false;
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    putReq.flush({});
    expect(router.navigate).toHaveBeenCalledWith(['/view-person-details', 'Updated']);
  });

  it('save with retire should create RETIRED duty then navigate', () => {
    component.editMode = true;
    component.editName = 'Vet';
    component.name = 'Vet';
    component.isAstronaut = true;
    component.dutySectionState = 'retire';
    component.allDuties = [{ rankLevel: 3 } as AstronautDuty];
    component.ranks = [{ id: 1, name: 'C', level: 3 }];
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    putReq.flush({});
    const dutyReq = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    expect(dutyReq.request.body.dutyTitle).toBe('RETIRED');
    dutyReq.flush({});
    expect(router.navigate).toHaveBeenCalledWith(['/view-person-details', 'Vet']);
  });

  it('save with newDuty should create duty then navigate', () => {
    component.editMode = true;
    component.editName = 'Person';
    component.name = 'Person';
    component.isAstronaut = true;
    component.dutySectionState = 'newDuty';
    component.dutyTitle = 'Captain';
    component.rankId = 1;
    component.dutyStartDate = '2024-06-01';
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    putReq.flush({});
    const dutyReq = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    dutyReq.flush({});
    expect(router.navigate).toHaveBeenCalledWith(['/view-person-details', 'Person']);
  });

  it('save should set error on updatePerson failure', () => {
    component.editMode = true;
    component.editName = 'X';
    component.name = 'Y';
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    putReq.flush({ message: 'Conflict' }, { status: 409, statusText: 'Conflict' });
    expect(component.error).toBeTruthy();
  });

  it('save should set error when duty op fails after update', () => {
    component.editMode = true;
    component.editName = 'Vet';
    component.name = 'Vet';
    component.isAstronaut = true;
    component.dutySectionState = 'retire';
    component.ranks = [{ id: 1, name: 'C', level: 3 }];
    component.allDuties = [{ rankLevel: 3 } as AstronautDuty];
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    putReq.flush({});
    const dutyReq = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    dutyReq.flush({ message: 'Duty failed' }, { status: 400, statusText: 'Bad Request' });
    expect(component.error).toBeTruthy();
  });

  it('submit when editMode calls save', () => {
    component.editMode = true;
    component.editName = 'A';
    component.name = 'A';
    component.submit();
    const putReq = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    expect(putReq).toBeTruthy();
    putReq.flush({});
  });
});
