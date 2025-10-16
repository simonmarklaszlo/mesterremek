import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  // Backend API URL
  private apiUrl = 'http://193.201.185.129:5432';

  constructor(private http: HttpClient) { }

  // Example: GET request to fetch data
  getData(endpoint: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${endpoint}`);
  }

  // Example: POST request to send data
  postData(endpoint: string, data: any): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.apiUrl}/${endpoint}`, data, { headers });
  }

  // Example: PUT request to update data
  updateData(endpoint: string, id: string, data: any): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.put(`${this.apiUrl}/${endpoint}/${id}`, data, { headers });
  }

  // Example: DELETE request
  deleteData(endpoint: string, id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${endpoint}/${id}`);
  }

  // Example: Custom query with parameters
  queryData(endpoint: string, params: any): Observable<any> {
    return this.http.get(`${this.apiUrl}/${endpoint}`, { params });
  }
}
