import { Component, OnDestroy, OnInit } from '@angular/core';
import { IMqttMessage, MqttService } from 'ngx-mqtt';
import { Subscription } from 'rxjs';
import { TimeSeriesService } from '../../services/time-series.service';
import * as mqtt from "mqtt"  // import everything inside the mqtt module and give it the namespace "mqtt"
@Component({
  selector: 'app-time-series-chart',
  templateUrl: './time-series-chart.component.html',
  styleUrls: ['./time-series-chart.component.scss']
})
export class TimeSeriesChartComponent implements OnInit, OnDestroy {
  grossProductData: any[] = [];
  interval: any;
  private subscription: Subscription = new Subscription();
  constructor(private service: TimeSeriesService) { }
  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  ngOnInit() {
    let client = mqtt.connect({
      port: 5000,
      protocol: 'ws',
      clientId: 'Time Series angular',
      path: '/mqtt'
    }) 
    // create a client
    client.on('connect', function () {
      console.log('connect')
      client.subscribe('/hello/data', function (err) {
        if (!err) {
          console.log('err')
        }
      })
    })
    client.on('message', (topic: any, message: any) =>  {
      // message is Buffer
      if (topic === '/hello/data') {
        let item = JSON.parse(message.toString());
        const itemFormat = {
          value : item.value,
          timestamp: new Date(item.timestamp).toDateString() + ' ' + new Date(item.timestamp).toLocaleTimeString()
        }
        this.grossProductData.push(itemFormat);
        console.log(message.toString(),item,this.grossProductData)

      }
    })
    // client.on('/hello/data', function (topic: any, message: any) {
    //   console.log('connect chart')
    //   // message is Buffer
    //   console.log(message.toString())
    //   client.end()
    // })
    
  }
  randomNumber(min: any, max: any) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }
}
