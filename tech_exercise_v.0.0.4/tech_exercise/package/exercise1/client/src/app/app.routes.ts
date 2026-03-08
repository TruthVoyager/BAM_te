import { Routes } from '@angular/router';
import { LandingComponent } from './pages/landing/landing.component';
import { ViewPeopleComponent } from './pages/view-people/view-people.component';
import { ViewPersonDetailsComponent } from './pages/view-person-details/view-person-details.component';
import { AddPersonComponent } from './pages/add-person/add-person.component';

export const routes: Routes = [
    { path: '', component: LandingComponent },
    { path: 'view-people', component: ViewPeopleComponent },
    { path: 'add-person', component: AddPersonComponent },
    { path: 'edit-person/:name', component: AddPersonComponent, data: { editMode: true } },
    { path: 'view-person-details/:name', component: ViewPersonDetailsComponent },
];
