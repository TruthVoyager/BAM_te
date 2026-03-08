import { BaseResponse } from './base-response';
import { PersonAstronaut } from './person-astronaut';

export interface GetPeopleResult extends BaseResponse {
  people: PersonAstronaut[];
}
