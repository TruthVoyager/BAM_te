export interface AstronautDuty {
  id: number;
  personId: number;
  rankId: number;
  rankLevel: number;
  rankName: string;
  dutyTitle: string;
  dutyStartDate: string;
  dutyEndDate: string | null;
}
