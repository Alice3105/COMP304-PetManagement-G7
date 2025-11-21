# User Actions and API Endpoints

## Public Account

### 1. Register for account

- **API Endpoint**: `POST /api/auth/register`
- **Details**: Creates a new user account (defaults to Public role)

### 2. Login to account

- **API Endpoint**: `POST /api/auth/login`
- **Details**: Authenticates user with email and password

### 3. Change account password

- **API Endpoint**: `PATCH /api/auth/password`
- **Details**: Changes user's own password (requires current password verification)
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 4. View all pets

- **API Endpoint**: `GET /api/pets`
- **Details**: Lists all available pets

### 5. View pet details

- **API Endpoint**: `GET /api/pets/{id}`
- **Details**: View detailed information about a specific pet

### 6. View medical records for a pet

- **API Endpoint**: `GET /api/medicalrecords/pet/{petId}`
- **Details**: View all medical records for a specific pet

### 7. Apply for adoption

- **API Endpoint**: `POST /api/adoptions`
- **Details**: Creates a new adoption application for an available pet
- **Note**: Must be logged in. Sets pet status to "Pending"

### 8. View own adoption applications

- **API Endpoint**: `GET /api/adoptions/user/{userId}`
- **Details**: View all adoption applications submitted by the logged-in user

### 9. View adoption application details

- **API Endpoint**: `GET /api/adoptions/{id}`
- **Details**: View details of a specific adoption application (own applications only)

### 10. Edit adoption application

- **API Endpoint**: `PUT /api/adoptions/{id}`
- **Details**: Update own adoption application (only if status is "Pending")
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 11. Delete adoption application

- **API Endpoint**: `DELETE /api/adoptions/{id}`
- **Details**: Delete own adoption application (only if status is "Pending")
- **Headers Required**: `X-User-Email`, `X-User-Role`

---

## Staff Account

### 1. Register for account

- **API Endpoint**: `POST /api/auth/register`
- **Details**: Creates a new user account (can specify Staff role during registration)

### 2. Login to account

- **API Endpoint**: `POST /api/auth/login`
- **Details**: Authenticates user with email and password

### 3. Change account password

- **API Endpoint**: `PATCH /api/auth/password`
- **Details**: Changes user's own password (requires current password verification)
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 4. View all pets

- **API Endpoint**: `GET /api/pets`
- **Details**: Lists all pets
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 5. View pet details

- **API Endpoint**: `GET /api/pets/{id}`
- **Details**: View detailed information about a specific pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 6. Add pet for adoption

- **API Endpoint**: `POST /api/pets` (multipart/form-data)
- **Details**: Creates a new pet record with optional photo upload
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 7. Edit pet info

- **API Endpoint (no photo)**: `PUT /api/pets/{id}`
- **API Endpoint (with photo)**: `PATCH /api/pets/{id}` (multipart/form-data)
- **Details**: Updates pet information. If photo is uploaded via PATCH, new photo is prepended to PhotoUrls list
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 8. Delete pet

- **API Endpoint**: `DELETE /api/pets/{id}`
- **Details**: Deletes a pet from the system
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 9. View medical records for a pet

- **API Endpoint**: `GET /api/medicalrecords/pet/{petId}`
- **Details**: View all medical records for a specific pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 10. Create medical record

- **API Endpoint**: `POST /api/medicalrecords`
- **Details**: Creates a new medical record for a pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 11. Edit medical record

- **API Endpoint**: `PUT /api/medicalrecords/{id}`
- **Details**: Updates an existing medical record
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 12. Delete medical record

- **API Endpoint**: `DELETE /api/medicalrecords/{id}`
- **Details**: Deletes a medical record
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 13. View all adoption applications

- **API Endpoint**: `GET /api/adoptions`
- **Details**: View all adoption applications (Staff can see all applications)

### 14. View adoption application details

- **API Endpoint**: `GET /api/adoptions/{id}`
- **Details**: View details of any adoption application

### 15. Change adoption application status

- **API Endpoint**: `PATCH /api/adoptions/{id}/status`
- **Details**: Updates adoption status (Pending, Approved, Rejected). Automatically updates pet status based on adoption status:
  - Approved → Pet status = "Adopted"
  - Rejected → Pet status updated based on other adoptions
  - Pending → Pet status = "Pending" (if no other approved adoptions)
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 16. Apply for adoption (self)

- **API Endpoint**: `POST /api/adoptions`
- **Details**: Staff can also apply for adoption (same as Public users)

### 17. Delete adoption application

- **API Endpoint**: `DELETE /api/adoptions/{id}`
- **Details**: Staff can delete any adoption application (any status). Automatically updates pet status based on remaining adoptions.
- **Headers Required**: `X-User-Email`, `X-User-Role`

---

## Admin Account

### 1. Register for account

- **API Endpoint**: `POST /api/auth/register`
- **Details**: Creates a new user account (can specify Admin role during registration)

### 2. Login to account

- **API Endpoint**: `POST /api/auth/login`
- **Details**: Authenticates user with email and password

### 3. Change account password

- **API Endpoint**: `PATCH /api/auth/password`
- **Details**: Changes user's own password (requires current password verification)
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 4. View all users

- **API Endpoint**: `GET /api/auth/users`
- **Details**: View all users in the system (Admin only)
- **Headers Required**: `X-User-Email`, `X-User-Role`
- **Authorization**: Requires Admin role validation

### 5. Edit user password/role

- **API Endpoint**: `PUT /api/auth/users/{userId}`
- **Details**: Admin can update any user's password or role
- **Headers Required**: `X-User-Email`, `X-User-Role`
- **Authorization**: Requires Admin role validation

### 6. Delete user

- **API Endpoint**: `DELETE /api/auth/users/{userId}`
- **Details**: Admin can delete any user from the system. Automatically deletes all adoption applications associated with the user and updates pet statuses accordingly.
- **Headers Required**: `X-User-Email`, `X-User-Role`
- **Authorization**: Requires Admin role validation

### 7. View all pets

- **API Endpoint**: `GET /api/pets`
- **Details**: Lists all pets
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 8. View pet details

- **API Endpoint**: `GET /api/pets/{id}`
- **Details**: View detailed information about a specific pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 9. Add pet for adoption

- **API Endpoint**: `POST /api/pets` (multipart/form-data)
- **Details**: Creates a new pet record with optional photo upload
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 10. Edit pet info

- **API Endpoint (no photo)**: `PUT /api/pets/{id}`
- **API Endpoint (with photo)**: `PATCH /api/pets/{id}` (multipart/form-data)
- **Details**: Updates pet information. If photo is uploaded via PATCH, new photo is prepended to PhotoUrls list
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 11. Delete pet

- **API Endpoint**: `DELETE /api/pets/{id}`
- **Details**: Deletes a pet from the system
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 12. View medical records for a pet

- **API Endpoint**: `GET /api/medicalrecords/pet/{petId}`
- **Details**: View all medical records for a specific pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 13. Create medical record

- **API Endpoint**: `POST /api/medicalrecords`
- **Details**: Creates a new medical record for a pet
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 14. Edit medical record

- **API Endpoint**: `PUT /api/medicalrecords/{id}`
- **Details**: Updates an existing medical record
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 15. Delete medical record

- **API Endpoint**: `DELETE /api/medicalrecords/{id}`
- **Details**: Deletes a medical record
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 16. View all adoption applications

- **API Endpoint**: `GET /api/adoptions`
- **Details**: View all adoption applications

### 17. View adoption application details

- **API Endpoint**: `GET /api/adoptions/{id}`
- **Details**: View details of any adoption application

### 18. Change adoption application status

- **API Endpoint**: `PATCH /api/adoptions/{id}/status`
- **Details**: Updates adoption status (Pending, Approved, Rejected). Automatically updates pet status based on adoption status
- **Headers Required**: `X-User-Email`, `X-User-Role`

### 19. Delete adoption application

- **API Endpoint**: `DELETE /api/adoptions/{id}`
- **Details**: Admin can delete any adoption application (any status). Automatically updates pet status based on remaining adoptions.
- **Headers Required**: `X-User-Email`, `X-User-Role`

---

## Summary

**Public users can:**

- Manage their own account (register, login, change password)
- Browse pets and medical records
- Create, view, edit (if Pending), and delete (if Pending) their own adoption applications

**Staff users can:**

- Everything Public users can do, PLUS:
- Manage pets (create, edit, delete, upload photos)
- Manage medical records (create, edit, delete)
- Update adoption application status
- View all adoption applications
- Delete any adoption application (any status)

**Admin users can:**

- Everything Staff users can do, PLUS:
- Manage users (view all, update password/role, delete - which also removes all user's adoption applications)
- Full system administration access
