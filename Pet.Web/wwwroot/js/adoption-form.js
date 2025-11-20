// Shared JavaScript for Adoption Create and Edit forms
// Handles conditional validation for OtherPetsDescription and ChildrenAges fields

(function() {
    'use strict';

    function initializeAdoptionForm() {
        const hasOtherPetsCheck = document.getElementById('hasOtherPetsCheck');
        const otherPetsDiv = document.getElementById('otherPetsDiv');
        const otherPetsDescription = document.getElementById('otherPetsDescription');
        const otherPetsRequired = document.getElementById('otherPetsRequired');

        const hasChildrenCheck = document.getElementById('hasChildrenCheck');
        const childrenAgesDiv = document.getElementById('childrenAgesDiv');
        const childrenAges = document.getElementById('childrenAges');
        const childrenAgesRequired = document.getElementById('childrenAgesRequired');

        // Handle HasOtherPets checkbox
        if (hasOtherPetsCheck && otherPetsDiv && otherPetsDescription && otherPetsRequired) {
            hasOtherPetsCheck.addEventListener('change', function() {
                if (this.checked) {
                    otherPetsDiv.style.display = 'block';
                    otherPetsDescription.setAttribute('required', 'required');
                    otherPetsRequired.style.display = 'inline';
                } else {
                    otherPetsDiv.style.display = 'none';
                    otherPetsDescription.removeAttribute('required');
                    otherPetsDescription.value = '';
                    otherPetsRequired.style.display = 'none';
                }
            });

            // Initialize on page load (in case checkbox is pre-checked)
            if (hasOtherPetsCheck.checked) {
                otherPetsDiv.style.display = 'block';
                otherPetsDescription.setAttribute('required', 'required');
                otherPetsRequired.style.display = 'inline';
            }
        }

        // Handle HasChildren checkbox
        if (hasChildrenCheck && childrenAgesDiv && childrenAges && childrenAgesRequired) {
            hasChildrenCheck.addEventListener('change', function() {
                if (this.checked) {
                    childrenAgesDiv.style.display = 'block';
                    childrenAges.setAttribute('required', 'required');
                    childrenAgesRequired.style.display = 'inline';
                } else {
                    childrenAgesDiv.style.display = 'none';
                    childrenAges.removeAttribute('required');
                    childrenAges.value = '';
                    childrenAgesRequired.style.display = 'none';
                }
            });

            // Initialize on page load (in case checkbox is pre-checked)
            if (hasChildrenCheck.checked) {
                childrenAgesDiv.style.display = 'block';
                childrenAges.setAttribute('required', 'required');
                childrenAgesRequired.style.display = 'inline';
            }
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeAdoptionForm);
    } else {
        initializeAdoptionForm();
    }
})();

