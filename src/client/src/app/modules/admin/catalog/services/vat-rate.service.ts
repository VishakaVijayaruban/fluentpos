import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { VatRateApiService } from 'src/app/core/api/catalog/vat-rate-api.service';
import { Result } from 'src/app/core/models/wrappers/Result';
import { VatRate } from '../models/vatRate';

@Injectable()
export class VatRateService {
  constructor(private api: VatRateApiService) {}

  getVatRates(): Observable<Result<VatRate[]>> {
    return this.api.getAlls().pipe(map((response: Result<VatRate[]>) => response));
  }
}
