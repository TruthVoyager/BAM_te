import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RankService } from './rank.service';

describe('RankService', () => {
  let service: RankService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [RankService],
    });
    service = TestBed.inject(RankService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getRanks should GET and return array', () => {
    const mock = [{ id: 1, name: 'Captain', level: 3 }];
    service.getRanks().subscribe((list) => {
      expect(Array.isArray(list)).toBe(true);
      expect(list.length).toBe(1);
      expect(list[0].name).toBe('Captain');
    });
    const req = httpMock.expectOne((r) => r.url.includes('/Rank') && r.method === 'GET');
    req.flush(mock);
  });
});
