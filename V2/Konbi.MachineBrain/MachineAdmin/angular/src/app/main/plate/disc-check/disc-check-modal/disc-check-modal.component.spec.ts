import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscCheckModalComponent } from './disc-check-modal.component';

describe('DiscCheckModalComponent', () => {
  let component: DiscCheckModalComponent;
  let fixture: ComponentFixture<DiscCheckModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiscCheckModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiscCheckModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
