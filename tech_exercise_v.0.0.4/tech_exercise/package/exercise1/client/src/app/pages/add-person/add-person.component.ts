import { Component, OnInit, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PersonService } from '../../shared/services/person.service';
import { RankService } from '../../shared/services/rank.service';
import { AstronautDutyService } from '../../shared/services/astronaut-duty.service';
import { GetAstronautDutiesByNameResult } from '../../shared/models/get-astronaut-duties-by-name-result';
import { Rank } from '../../shared/models/rank';
import { AstronautDuty } from '../../shared/models/astronaut-duty';
import { LoadingSpinnerComponent } from '../../components/loading-spinner/loading-spinner.component';
import { getErrorMessage } from '../../shared/get-error-message';

type DutySectionState = 'readonly' | 'newDuty' | 'retire' | 'promote';

@Component({
  selector: 'app-add-person',
  imports: [FormsModule, LoadingSpinnerComponent],
  templateUrl: './add-person.component.html',
  styleUrl: './add-person.component.css',
})
export class AddPersonComponent implements OnInit, OnDestroy {
  name = '';
  isAstronaut = false;
  dutyTitle = '';
  rankId: number | null = null;
  dutyStartDate = '';

  ranks: Rank[] = [];
  loadingRanks = true;
  loadingPerson = false;
  submitting = false;
  error: string | null = null;

  editMode = false;
  editName: string | null = null;
  currentDuty: AstronautDuty | null = null;
  allDuties: AstronautDuty[] = [];
  dutySectionState: DutySectionState = 'readonly';
  isAstronautReadonly = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly location: Location,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly personService: PersonService,
    private readonly rankService: RankService,
    private readonly astronautDutyService: AstronautDutyService,
  ) {}

  ngOnInit(): void {
    this.rankService.getRanks().subscribe({
      next: (list) => {
        const raw = Array.isArray(list) ? list : [];
        this.ranks = raw.map((r: { id?: number; name?: string; level?: number; Id?: number; Name?: string; Level?: number }) => ({
          id: r.id ?? r.Id!,
          name: r.name ?? r.Name ?? '',
          level: r.level ?? r.Level ?? 0,
        }));
        this.loadingRanks = false;
        if (this.ranks.length > 0 && this.rankId == null) {
          this.rankId = this.ranks[0].id;
        }
      },
      error: (err) => {
        this.loadingRanks = false;
        this.error = getErrorMessage(err, 'Failed to load ranks. Is the API running?');
      },
    });

    this.route.data.pipe(takeUntil(this.destroy$)).subscribe(() => this.applyRouteState());
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(() => this.applyRouteState());

    // Run immediately for initial route (observables may not emit on first subscribe)
    this.applyRouteState();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private applyRouteState(): void {
    this.editMode = this.route.snapshot.data['editMode'] === true;
    this.editName = this.route.snapshot.paramMap.get('name');
    if (this.editMode && this.editName) {
      if (this.loadingPerson) return;
      const decodedName = decodeURIComponent(this.editName);
      this.loadingPerson = true;
      this.error = null;
      this.astronautDutyService.getDutiesByName(decodedName).subscribe({
        next: (res: GetAstronautDutiesByNameResult) => {
          this.loadingPerson = false;
          const success = res.success !== false;
          const person = res.person;
          const duties = res.astronautDuties ?? [];
          if (!success || !person) {
            this.error = res.message ?? 'Person not found';
            return;
          }
          this.name = person.name;
          this.allDuties = duties;
          this.currentDuty = this.allDuties.find((d) => d.dutyEndDate == null) ?? null;
          this.isAstronaut = this.allDuties.length > 0 || (person.careerStartDate != null && person.careerStartDate !== '');
          this.isAstronautReadonly = this.isAstronaut;
          this.resetDutyDisplayToCurrent();
        },
        error: (err) => {
          this.loadingPerson = false;
          this.error = getErrorMessage(err, 'Failed to load person');
        },
      });
    } else {
      this.loadingPerson = false;
      this.name = '';
      this.isAstronaut = false;
      this.isAstronautReadonly = false;
      this.editName = null;
      this.currentDuty = null;
      this.allDuties = [];
      this.dutySectionState = 'readonly';
      this.dutyTitle = '';
      this.rankId = this.ranks.length > 0 ? this.ranks[0].id : null;
      this.dutyStartDate = new Date().toISOString().slice(0, 10);
    }
  }

  get isDutyReadonly(): boolean {
    return this.dutySectionState === 'readonly' || this.dutySectionState === 'retire';
  }
  get isDutyFieldsRequired(): boolean {
    return this.isAstronaut && (this.dutySectionState === 'newDuty' || !this.editMode);
  }

  get isPromoteRankRequired(): boolean {
    return this.isAstronaut && this.dutySectionState === 'promote';
  }

  get isSubmitDisabled(): boolean {
    if (this.submitting) return true;
    if (this.loadingRanks && (this.isDutyFieldsRequired || this.isPromoteRankRequired)) return true;
    if (this.dutySectionState === 'promote' && (this.rankId == null || this.rankId === this.currentDuty?.rankId)) return true;

    // Mirror required-field rules without relying on ViewChild timing.
    if (!this.name?.trim()) return true;

    if (this.isDutyFieldsRequired) {
      if (!this.dutyTitle?.trim() || this.rankId == null) return true;
      if (this.editMode && this.dutySectionState === 'newDuty' && !this.dutyStartDate) return true;
    }

    return false;
  }

  get displayDutyTitle(): string {
    if (this.dutySectionState === 'retire') return 'RETIRED';
    if ((this.dutySectionState === 'readonly' || this.dutySectionState === 'promote') && this.currentDuty) return this.currentDuty.dutyTitle;
    return this.dutyTitle;
  }

  get displayRankId(): number | null {
    if (this.dutySectionState === 'retire') return this.highestRankId;
    if (this.dutySectionState === 'readonly' && this.currentDuty) return this.currentDuty.rankId;
    return this.rankId;
  }

  get displayRankName(): string {
    if (this.dutySectionState === 'retire' && this.highestRankId != null) {
      const r = this.ranks.find((x) => x.id === this.highestRankId);
      return r?.name ?? '';
    }
    if (this.dutySectionState === 'readonly' && this.currentDuty) return this.currentDuty.rankName;
    const r = this.ranks.find((x) => x.id === this.rankId);
    return r?.name ?? '';
  }

  get highestRankId(): number | null {
    if (this.allDuties.length === 0) return null;
    const maxLevel = Math.max(...this.allDuties.map((d) => d.rankLevel ?? 0));
    const maxRanksLevel = this.ranks.length > 0 ? Math.max(...this.ranks.map((x) => x.level)) : 0;
    const rank = this.ranks.find((r) => r.level === maxLevel) ?? this.ranks.find((r) => r.level === maxRanksLevel);
    return rank?.id ?? null;
  }

  resetDutyDisplayToCurrent(): void {
    this.dutySectionState = 'readonly';
    if (this.currentDuty) {
      this.dutyTitle = this.currentDuty.dutyTitle;
      this.rankId = this.currentDuty.rankId;
      this.dutyStartDate = this.currentDuty.dutyStartDate?.toString().slice(0, 10) ?? new Date().toISOString().slice(0, 10);
    } else {
      this.dutyTitle = '';
      this.rankId = this.ranks.length > 0 ? this.ranks[0].id : null;
      this.dutyStartDate = new Date().toISOString().slice(0, 10);
    }
  }

  startNewDuty(): void {
    this.dutySectionState = 'newDuty';
    this.dutyTitle = '';
    this.rankId = this.ranks.length > 0 ? this.ranks[0].id : null;
    this.dutyStartDate = new Date().toISOString().slice(0, 10);
  }

  showRetire(): void {
    this.dutySectionState = 'retire';
    this.dutyTitle = 'RETIRED';
    this.rankId = this.highestRankId;
  }

  showPromote(): void {
    this.dutySectionState = 'promote';
    this.rankId = this.currentDuty?.rankId ?? this.rankId;
  }

  cancelDutyEdit(): void {
    this.resetDutyDisplayToCurrent();
  }

  cancel(): void {
    this.location.back();
  }

  create(): void {
    const trimmed = this.name?.trim();
    if (!trimmed) {
      this.error = 'Name is required';
      return;
    }
    if (this.isAstronaut && (!this.dutyTitle?.trim() || this.rankId == null)) {
      this.error = 'Duty title and rank are required when adding an astronaut';
      return;
    }
    this.error = null;
    this.submitting = true;

    this.personService.createPerson(trimmed).subscribe({
      next: () => {
        if (this.isAstronaut && this.rankId != null) {
          this.astronautDutyService
            .createDuty({
              name: trimmed,
              rankId: Number(this.rankId),
              dutyTitle: this.dutyTitle.trim(),
              dutyStartDate: this.dutyStartDate || new Date().toISOString().slice(0, 10),
            })
            .subscribe({
              next: () => {
                this.submitting = false;
                this.router.navigate(['/view-people']);
              },
              error: (err) => {
                this.submitting = false;
                this.error = getErrorMessage(err, 'Person created but failed to add duty');
              },
            });
        } else {
          this.submitting = false;
          this.router.navigate(['/view-people']);
        }
      },
      error: (err) => {
        this.submitting = false;
        this.error = getErrorMessage(err, 'Failed to create person');
      },
    });
  }

  save(): void {
    const trimmed = this.name?.trim();
    if (!trimmed || !this.editName) return;
    if (this.isAstronaut && this.dutySectionState === 'newDuty' && (!this.dutyTitle?.trim() || this.rankId == null)) {
      this.error = 'Duty title and rank are required for new duty';
      return;
    }
    if (this.isAstronaut && this.dutySectionState === 'promote' && (this.rankId == null || this.currentDuty == null)) {
      this.error = 'Select a new rank to promote.';
      return;
    }
    if (this.isAstronaut && this.dutySectionState === 'retire' && this.highestRankId == null) {
      this.error = 'Unable to determine rank for retirement. Please refresh and try again.';
      return;
    }
    this.error = null;
    this.submitting = true;

    this.personService.updatePerson(this.editName, trimmed).subscribe({
      next: () => {
        const nextOp =
          this.isAstronaut && this.dutySectionState === 'retire'
            ? this.astronautDutyService.createDuty({
                name: trimmed,
                rankId: this.highestRankId!,
                dutyTitle: 'RETIRED',
                dutyStartDate: new Date().toISOString().slice(0, 10),
              })
            : this.isAstronaut && this.dutySectionState === 'newDuty' && this.dutyTitle?.trim() && this.rankId != null
              ? this.astronautDutyService.createDuty({
                  name: trimmed,
                  rankId: Number(this.rankId),
                  dutyTitle: this.dutyTitle.trim(),
                  dutyStartDate: this.dutyStartDate || new Date().toISOString().slice(0, 10),
                })
              : this.isAstronaut && this.dutySectionState === 'promote' && this.currentDuty != null && this.rankId != null && this.rankId !== this.currentDuty.rankId
                ? this.astronautDutyService.updateDutyRank(this.currentDuty.id, Number(this.rankId))
                : null;

        if (nextOp) {
          nextOp.subscribe({
            next: () => {
              this.submitting = false;
              this.router.navigate(['/view-person-details', trimmed]);
            },
            error: (err) => {
              this.submitting = false;
              this.error = getErrorMessage(err, 'Person updated but duty failed');
            },
          });
        } else {
          this.submitting = false;
          this.router.navigate(['/view-person-details', trimmed]);
        }
      },
      error: (err) => {
        this.submitting = false;
        this.error = getErrorMessage(err, 'Failed to update person');
      },
    });
  }

  submit(): void {
    if (this.editMode) this.save();
    else this.create();
  }
}
