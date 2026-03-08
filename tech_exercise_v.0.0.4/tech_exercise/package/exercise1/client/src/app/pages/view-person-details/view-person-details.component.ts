import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AstronautDutyService } from '../../shared/services/astronaut-duty.service';
import { AstronautDuty } from '../../shared/models/astronaut-duty';
import { PersonAstronaut } from '../../shared/models/person-astronaut';
import { DutyCardComponent } from '../../components/duty-card/duty-card.component';

@Component({
  selector: 'app-view-person-details',
  imports: [RouterLink, DutyCardComponent],
  templateUrl: './view-person-details.component.html',
  styleUrl: './view-person-details.component.css',
})
export class ViewPersonDetailsComponent implements OnInit {
  person: PersonAstronaut | null = null;
  currentDuty: AstronautDuty | null = null;
  pastDuties: AstronautDuty[] = [];
  loading = true;
  error: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly astronautDutyService: AstronautDutyService,
  ) {}

  ngOnInit(): void {
    const name = this.route.snapshot.paramMap.get('name');
    if (!name) {
      this.loading = false;
      this.error = 'No person specified';
      return;
    }
    this.astronautDutyService.getDutiesByName(name).subscribe({
      next: (res) => {
        this.loading = false;
        if (!res.success || !res.person) {
          this.error = res.message ?? 'Failed to load person';
          return;
        }
        this.person = res.person;
        const duties = res.astronautDuties ?? [];
        this.currentDuty = duties.find((d) => d.dutyEndDate == null) ?? null;
        this.pastDuties = duties.filter((d) => d.dutyEndDate != null);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.message ?? 'Failed to load person details';
      },
    });
  }
}
