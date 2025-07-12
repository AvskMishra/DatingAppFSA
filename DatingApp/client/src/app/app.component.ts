import { NgFor } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TooltipModule } from 'ngx-bootstrap/tooltip';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet,NgFor,TooltipModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  http = inject(HttpClient); //new way to inject HttpClient in Angular inctead of constructor injection
  title = 'Dating App';
  users:any;

  ngOnInit(): void {
    this.http.get('https://localhost:7169/api/Users').subscribe({
      next: res=>this.users =res,
      error: err=> console.log(err),
      complete: () => console.log('Request completed'),
    });
    // constructor(private http: HttpClient) { }
  }
}
