import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService, ModalOptions } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_modules/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {

  users:User[]=[];
  bsModalRef: BsModalRef<RolesModalComponent> = new BsModalRef<RolesModalComponent>();
  availableRoles = [
    'Admin',
    'Moderator',
    'Member'
  ]

  constructor(private adminService: AdminService, private modalService:BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles()
  {
    this.adminService.getUsersWithRoles().subscribe({
      next: users => this.users = users
    })
  }

  openRolesModal(user: User)
  {
   const config = {
    class: 'modal-dialog-centered',
    initialState:{
      username: user.username,
      availableRoles: this.availableRoles,
      selectedRoles: [...user.roles] //By using the spread operator, the elements of user.roles are spread out into the new array,
      // rather than just copying the reference to user.roles. 
      //This means that any changes made to selectedRoles will not affect user.roles.
    }
   }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.onHide?.subscribe({
      next: () => {
        const selectedRoles = this.bsModalRef.content?.selectedRoles;
        if(!this.arrayEqual( selectedRoles, user.roles)){
          this.adminService.updateUserRoles(user.username, selectedRoles.join(',')).subscribe(
            {
              next: roles => user.roles = roles
            }
          )
        }
      }
    })
  }

  private arrayEqual(arr1: any[], arr2: any[])
  {
    return JSON.stringify(arr1.sort()) === JSON.stringify(arr2.sort());
  }

}
