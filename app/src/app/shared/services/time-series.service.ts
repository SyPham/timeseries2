import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { createStore } from 'devextreme-aspnet-data-nojquery';
import DataSource from 'devextreme/data/data_source';
let API_URL ="http://localhost:5000/api";
@Injectable({
  providedIn: 'root'
})
export class TimeSeriesService {
  entity = 'TimeSeries';
constructor(private http: HttpClient) { }
add(entity: any): Observable<any> {
  return this.http.post<any>(`${API_URL}/${this.entity}/Add`, entity);
}
update(entity: any): Observable<any> {
  return this.http.put<any>(`${API_URL}/${this.entity}/Update`, entity);
}
remove(id: any): Observable<any> {
  return this.http.delete<any>(`${API_URL}/${this.entity}/Delete`, { params: { key: id } });
}

getAll() {
  return this.http.get<any>(`${API_URL}/${this.entity}/Get`);
}
loadStoreLookup() {

  let self = this;
  return {
    store: createStore({
      key: "Id",
      loadUrl: `${API_URL}/${this.entity}/LoadDxoLookup`,
      onBeforeSend: function (method, ajaxOptions) {
        ajaxOptions.headers = {
          crossdomain: true,
         // Authorization: `Bearer ${self.authService.currentUser.token}`
        }
      }
    }),
    paginate: true,
    pageSize: 10
  }
}

loadDataGrid() {

  let self = this;
  return new DataSource({
    store: createStore({
      key: `Id`,
      loadUrl: `${API_URL}/${this.entity}/LoadDxoGrid`,
      deleteUrl: `${API_URL}/${this.entity}/Delete`,
      updateUrl: `${API_URL}/${this.entity}/Update`,
      insertUrl: `${API_URL}/${this.entity}/Add`,
      onBeforeSend: function (method, ajaxOptions) {
        ajaxOptions.headers = {
          //Authorization: `Bearer ${self.authService.currentUser.token}`
        }
      }
    }),
    map: (item) => {
      // item.Status = item.Status==true?1:0;
      // item.Avatar = 'api/'+item.Avatar.replace("{0}","Sm")
      return item;
    }
  });
}
}
