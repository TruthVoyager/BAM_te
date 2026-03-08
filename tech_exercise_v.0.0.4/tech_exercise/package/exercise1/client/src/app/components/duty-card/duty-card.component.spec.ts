import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DutyCardComponent } from './duty-card.component';
import { AstronautDuty } from '../../shared/models/astronaut-duty';

describe('DutyCardComponent', () => {
  let fixture: ComponentFixture<DutyCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DutyCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DutyCardComponent);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display duty title, rank, start date and end date', () => {
    const duty: AstronautDuty = {
      id: 1,
      personId: 1,
      rankId: 2,
      rankLevel: 2,
      rankName: 'Sergeant',
      dutyTitle: 'Pilot',
      dutyStartDate: '2024-01-15',
      dutyEndDate: null,
    };
    fixture.componentRef.setInput('duty', duty);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('Pilot');
    expect(el.textContent).toContain('Sergeant');
    expect(el.textContent).toContain('Current');
  });

  it('should display end date when dutyEndDate is set', () => {
    const duty: AstronautDuty = {
      id: 1,
      personId: 1,
      rankId: 1,
      rankLevel: 1,
      rankName: 'Captain',
      dutyTitle: 'Commander',
      dutyStartDate: '2023-01-01',
      dutyEndDate: '2024-06-30',
    };
    fixture.componentRef.setInput('duty', duty);
    fixture.detectChanges();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.textContent).toContain('Commander');
    expect(el.textContent).toContain('Captain');
    expect(el.textContent).toContain('2024');
    expect(el.textContent).not.toContain('Current');
  });
});
