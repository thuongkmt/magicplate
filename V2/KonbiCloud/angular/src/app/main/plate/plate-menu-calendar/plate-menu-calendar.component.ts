import { Component, OnInit, ViewEncapsulation, Injector, ChangeDetectionStrategy, ViewChild, TemplateRef, ChangeDetectorRef } from '@angular/core';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Router } from '@angular/router';
import { startOfDay, endOfDay, subDays, addDays, endOfMonth, isSameDay, isSameMonth, addHours } from 'date-fns';
import { Subject } from 'rxjs';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CalendarEvent, CalendarEventAction, CalendarEventTimesChangedEvent, CalendarView } from 'angular-calendar';
import { PlateMenuDayComponent } from './plate-menu-day/plate-menu-day.component';
import { PlateMenuServiceProxy, PlateMenuMonthResult } from '@shared/service-proxies/plate-menu-service-proxies';
import { List } from 'lodash';

const colors: any = {
  red: {
    primary: '#ad2121',
    secondary: '#FAE3E3'
  },
  blue: {
    primary: '#1e90ff',
    secondary: '#D1E8FF'
  },
  yellow: {
    primary: '#e3bc08',
    secondary: '#FDF1BA'
  }
};


@Component({
  selector: 'app-plate-menu-calendar',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './plate-menu-calendar.component.html',
  styleUrls: ['./plate-menu-calendar.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class PlateMenuCalendarComponent extends AppComponentBase {

  @ViewChild('plateMenuDayComponent') plateMenuDayComponent: PlateMenuDayComponent;
  viewDate: Date = new Date();
  view: CalendarView = CalendarView.Month;
  CalendarView = CalendarView;

  events: CalendarEvent[] = [];
  public lstDayNoMenuOfWeek: String[] = [];
  isLoading = false;

  constructor(
    injector: Injector,
    private _plateMenusServiceProxy: PlateMenuServiceProxy,
    private changeDetectorRef: ChangeDetectorRef,
    private router: Router,

  ) {
    super(injector);
  }

  /**
   * Init.
   */
  ngOnInit() {
    this.getNext7DayNoMenu();
    this.getMonthPlateMenuStatus();
  }

  /**
   * Get next 7 day.
   */
  getNext7DayNoMenu() {
    this._plateMenusServiceProxy.GetWeekPlateMenus().subscribe(result => {
      if (result != null) {
        this.lstDayNoMenuOfWeek = result;
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  /**
   * Get month plate menus status.
   */
  getMonthPlateMenuStatus() {
    let month = ('0' + (this.viewDate.getMonth() + 1)).slice(-2);
    let day = ('0' + this.viewDate.getDate()).slice(-2);
    let seletedDate = month + '/' + day + '/' + this.viewDate.getFullYear();
    this.primengTableHelper.showLoadingIndicator();
    this.isLoading = true;
    this._plateMenusServiceProxy.GetMonthPlateMenus(seletedDate).subscribe(result => {
      this.isLoading = false;
      this.primengTableHelper.showLoadingIndicator();
      if (result != null) {
        this.setEventCalendar(result);
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  /**
   * Set event calendar.
   * @param result result plate menu month.
   */
  setEventCalendar(result: PlateMenuMonthResult[]) {
    this.events = [];
    result.forEach(element => {
      if (element.totalNoPrice > 0) {
        this.events.push({
          title: 'Some meals have not been set',
          start: new Date(element.day),
          color: colors.yellow
        });
      } else {
        this.events.push({
          title: 'The meal has been set',
          start: new Date(element.day),
          color: colors.blue
        });
      }
    });
  }

  /**
   * Click date in calendar.
   * @param param0 Date.
   */
  dayClicked({ date, events }: { date: Date; events: CalendarEvent[] }): void {
    this.plateMenuDayComponent.show(date);
  }

  /**
   * Get router link.
   * @param item Link.
   */
  getRouterLink(item) {
    let items = item.split('/');
    let routerUrl = 'app/main/plate/plateMenus/' + items[2] + '-' + items[1] + '-' + items[0];
    this.router.navigate([routerUrl]);
  }

  /**
   * Calendar change.
   */
  CalendarChange() {
    this.getMonthPlateMenuStatus();
  }
}
