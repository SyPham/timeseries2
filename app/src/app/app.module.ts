import { CustomerComponent } from './pages/customer/customer.component';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { SideNavOuterToolbarModule, SideNavInnerToolbarModule, SingleCardModule } from './layouts';
import { FooterModule, ResetPasswordFormModule, CreateAccountFormModule, ChangePasswordFormModule, LoginFormModule } from './shared/components';
import { AuthService, ScreenService, AppInfoService } from './shared/services';
import { UnauthenticatedContentModule } from './unauthenticated-content';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';
import { DxButtonModule, DxChartModule, DxCheckBoxModule, DxColorBoxModule, DxDataGridModule, DxDraggableModule, DxFileUploaderModule, DxFormModule, DxListModule, DxLoadIndicatorModule, DxLoadPanelModule, DxPopupModule, DxResponsiveBoxModule, DxScrollViewModule, DxSelectBoxModule, DxSwitchModule, DxTabsModule, DxTagBoxModule, DxTextAreaModule, DxTextBoxModule, DxToastModule, DxToolbarModule } from 'devextreme-angular';
import { FormsModule } from '@angular/forms';
import { TimeSeriesChartComponent } from './shared/components/time-series-chart/time-series-chart.component';
import {
  IMqttMessage,
  MqttModule,
  IMqttServiceOptions
} from 'ngx-mqtt';

export const MQTT_SERVICE_OPTIONS: IMqttServiceOptions = {
  hostname: 'localhost',
  connectOnCreate: true,
  port: 5000,
  path: '/mqtt',
  protocol: 'ws',
  clientId: 'henry-angular'
};


const DEVTREME_MODULE = [
    DxFormModule,
    DxTextAreaModule,
    DxResponsiveBoxModule,
    DxToastModule,
    DxButtonModule,
    DxFileUploaderModule,
    DxTextBoxModule,
    DxTagBoxModule,
    DxDataGridModule,
    DxListModule,
    DxDraggableModule,
    DxScrollViewModule,
    DxPopupModule,
    DxCheckBoxModule,
    DxToolbarModule,
    DxSelectBoxModule,
    DxColorBoxModule,
    DxSwitchModule,
    DxTabsModule,
    DxLoadIndicatorModule,
    DxLoadPanelModule,
    DxChartModule
]
@NgModule({
  declarations: [
    AppComponent,
    CustomerComponent,
    TimeSeriesChartComponent
  ],
  imports: [
    BrowserModule,
    SideNavOuterToolbarModule,
    SideNavInnerToolbarModule,
    SingleCardModule,
    FooterModule,
    ResetPasswordFormModule,
    CreateAccountFormModule,
    ChangePasswordFormModule,
    LoginFormModule,
    UnauthenticatedContentModule,
    AppRoutingModule,
    HttpClientModule,
    ...DEVTREME_MODULE,
    FormsModule,
  ],
  providers: [AuthService, ScreenService, AppInfoService],
  bootstrap: [AppComponent]
})
export class AppModule { }
