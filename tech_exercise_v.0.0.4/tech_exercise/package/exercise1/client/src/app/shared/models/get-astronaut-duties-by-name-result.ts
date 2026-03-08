import { BaseResponse } from './base-response';
import { PersonAstronaut } from './person-astronaut';
import { AstronautDuty } from './astronaut-duty';

export interface GetAstronautDutiesByNameResult extends BaseResponse {
  person: PersonAstronaut | null;
  astronautDuties: AstronautDuty[];
}
