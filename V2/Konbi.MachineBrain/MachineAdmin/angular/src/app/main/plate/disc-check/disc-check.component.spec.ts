import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DiscCheckComponent } from './disc-check.component';

describe('DiscCheckComponent', () => {
  let component: DiscCheckComponent;
  let fixture: ComponentFixture<DiscCheckComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DiscCheckComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DiscCheckComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
