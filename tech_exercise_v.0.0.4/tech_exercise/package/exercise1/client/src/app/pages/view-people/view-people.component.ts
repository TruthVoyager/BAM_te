import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PersonService } from '../../shared/services/person.service';
import { PersonAstronaut } from '../../shared/models/person-astronaut';

@Component({
  selector: 'app-view-people',
  imports: [RouterLink],
  templateUrl: './view-people.component.html',
  styleUrl: './view-people.component.css',
})
export class ViewPeopleComponent implements OnInit {
  people: PersonAstronaut[] = [];
  loading = true;
  error: string | null = null;

  constructor(private readonly personService: PersonService) {}

  ngOnInit(): void {
    this.personService.getPeople().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.people) {
          this.people = res.people;
        } else {
          this.error = res.message ?? 'Failed to load people';
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.message ?? 'Failed to load people';
      },
    });
  }
}
