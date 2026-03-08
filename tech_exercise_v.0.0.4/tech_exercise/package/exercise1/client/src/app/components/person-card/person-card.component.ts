import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PersonAstronaut } from '../../shared/models/person-astronaut';

@Component({
  selector: 'app-person-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './person-card.component.html',
  styleUrl: './person-card.component.css',
})
export class PersonCardComponent {
  person = input.required<PersonAstronaut>();

  filledStars(): number {
    const level = this.person()?.currentRankLevel ?? 0;
    return Math.min(Math.max(0, level), 5);
  }
}
