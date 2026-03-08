import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PersonService } from './person.service';

describe('PersonService', () => {
  let service: PersonService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PersonService],
    });
    service = TestBed.inject(PersonService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getPeople should GET and return result', () => {
    const mock = { success: true, people: [{ personId: 1, name: 'Test' }] };
    service.getPeople().subscribe((res) => {
      expect(res.success).toBe(true);
      expect(res.people?.length).toBe(1);
      expect(res.people?.[0].name).toBe('Test');
    });
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'GET');
    req.flush(mock);
  });

  it('getPersonByName should GET with encoded name', () => {
    const mock = { success: true, person: { personId: 1, name: 'Jane Doe' } };
    service.getPersonByName('Jane Doe').subscribe((res) => {
      expect(res.person?.name).toBe('Jane Doe');
    });
    const req = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'GET');
    expect(req.request.url).toContain(encodeURIComponent('Jane Doe'));
    req.flush(mock);
  });

  it('createPerson should POST with JSON body', () => {
    service.createPerson('New Person').subscribe();
    const req = httpMock.expectOne((r) => r.url.includes('/Person') && r.method === 'POST');
    expect(req.request.body).toEqual(JSON.stringify('New Person'));
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush({});
  });

  it('updatePerson should PUT with encoded currentName and JSON newName', () => {
    service.updatePerson('Old Name', 'New Name').subscribe();
    const req = httpMock.expectOne((r) => r.url.includes('/Person/') && r.method === 'PUT');
    expect(req.request.url).toContain(encodeURIComponent('Old Name'));
    expect(req.request.body).toEqual(JSON.stringify('New Name'));
    req.flush({});
  });
});
