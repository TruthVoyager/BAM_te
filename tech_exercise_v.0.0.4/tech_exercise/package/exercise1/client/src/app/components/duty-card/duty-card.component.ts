import { Component, input } from '@angular/core';
import { AstronautDuty } from '../../shared/models/astronaut-duty';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-duty-card',
  imports: [DatePipe],
  templateUrl: './duty-card.component.html',
  styleUrl: './duty-card.component.css',
})
export class DutyCardComponent {
  duty = input.required<AstronautDuty>();
}
