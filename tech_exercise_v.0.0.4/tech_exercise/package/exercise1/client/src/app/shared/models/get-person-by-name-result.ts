import { BaseResponse } from './base-response';
import { PersonAstronaut } from './person-astronaut';

export interface GetPersonByNameResult extends BaseResponse {
  person: PersonAstronaut | null;
}
