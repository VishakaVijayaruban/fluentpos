import {HttpClient} from '@angular/common/http';
import {Injectable} from '@angular/core';
import { VatRate } from 'src/app/modules/admin/catalog/models/vatRate';
import {environment} from 'src/environments/environment';
import { Result } from '../../models/wrappers/Result';

@Injectable()
export class VatRateApiService {

  baseUrl = environment.apiUrl + 'catalog/vatrates/';

  constructor(private http: HttpClient) {
  }

  getAlls() {
    return this.http.get<Result<VatRate[]>>(this.baseUrl);
  }
}
