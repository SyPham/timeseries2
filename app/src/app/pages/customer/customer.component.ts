import { CustomerService } from './../../shared/services/customer.service';
import { Component, OnInit } from '@angular/core';
import 'devextreme/data/odata/store';
@Component({
  selector: 'app-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.scss']
})
export class CustomerComponent implements OnInit {
  dataSource: any;
  constructor(private service: CustomerService) {
    this.dataSource = this.service.loadDataGrid()
   }
  ngOnInit() {
  }
  onRowRemoved(e: any) {
    console.log(e.error.message);
  }
}
