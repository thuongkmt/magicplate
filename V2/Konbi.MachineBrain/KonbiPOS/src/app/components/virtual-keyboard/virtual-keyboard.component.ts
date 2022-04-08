import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import * as r from 'ramda';

const backspace = 'BACKSPACE';
const enter = 'ENTER';
const dot = '.';
const prefix = '$';

@Component({
    selector: 'pos-virtual-keyboard',
    templateUrl: './virtual-keyboard.component.html',
    styleUrls: ['./virtual-keyboard.component.scss']
})
export class VirtualKeyboardComponent implements OnInit {
    @Output() virKeyboardOutputValue: EventEmitter<any> = new EventEmitter();
    inputValue: string = '';
    inputDisplay: string = '';

    constructor() { }

    ngOnInit(): void { }

    keyPress = (key) => {
        if (r.toUpper(key) === backspace) {
            if (!r.isEmpty(this.inputValue)) {
                this.inputValue = r.dropLast(1, this.inputValue);
                this.toDisplay();
                return;
            }
        }

        if (r.toUpper(key) === enter) {
            this.virKeyboardOutputValue.emit(this.inputValue);
            this.inputValue = r.empty(this.inputValue);
            this.toDisplay();
            return;

        }

        if (!r.isEmpty(this.inputValue) && this.inputValue.length > 5) return;

        if (key === dot && r.isEmpty(this.inputValue)) return;

        if (r.toUpper(key) !== backspace && r.toUpper(key) !== enter) {
            this.inputValue += key;
            this.toDisplay();
        }
    }

    toDisplay = () => {
        this.inputDisplay = r.concat(prefix, r.isEmpty(this.inputValue) ? '0.00' : this.inputValue);
    }
}
