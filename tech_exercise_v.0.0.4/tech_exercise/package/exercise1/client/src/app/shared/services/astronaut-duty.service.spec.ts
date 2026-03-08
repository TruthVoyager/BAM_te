import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AstronautDutyService } from './astronaut-duty.service';

describe('AstronautDutyService', () => {
  let service: AstronautDutyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AstronautDutyService],
    });
    service = TestBed.inject(AstronautDutyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getDutiesByName should GET with encoded name', () => {
    const mock = {
      success: true,
      person: { name: 'Astro' },
      astronautDuties: [{ id: 1, dutyTitle: 'Pilot', dutyEndDate: null }],
    };
    service.getDutiesByName('Astro').subscribe((res) => {
      expect(res.person?.name).toBe('Astro');
      expect(res.astronautDuties?.length).toBe(1);
    });
    const req = httpMock.expectOne((r) => r.url.includes('/AstronautDuty/') && r.method === 'GET');
    expect(req.request.url).toContain(encodeURIComponent('Astro'));
    req.flush(mock);
  });

  it('createDuty should POST request body', () => {
    const body = { name: 'Test', rankId: 1, dutyTitle: 'Captain', dutyStartDate: '2024-01-01' };
    service.createDuty(body).subscribe();
    const req = httpMock.expectOne((r) => r.url.includes('/AstronautDuty') && r.method === 'POST');
    expect(req.request.body).toEqual(body);
    req.flush({});
  });
});
