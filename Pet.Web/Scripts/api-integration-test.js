/**
 * API Integration Test Script
 * 
 * This script can be run in the browser console to verify that the web app
 * is correctly configured to consume the API.
 * 
 * Usage: Copy and paste this script into the browser console while on the web app.
 */

(function() {
    'use strict';

    const API_BASE_URL = 'https://136.110.184.29.nip.io/v1/shelter';
    const API_KEY = 'DEV1-PET-ABC123XYZ';

    console.log('=== Pet.Web API Integration Test ===\n');
    console.log('API Base URL:', API_BASE_URL);
    console.log('API Key:', API_KEY);
    console.log('\n');

    /**
     * Test API connectivity
     */
    async function testApiConnectivity() {
        console.log('Testing API Connectivity...');
        
        try {
            const response = await fetch(`${API_BASE_URL}/api/pets`, {
                method: 'GET',
                headers: {
                    'x-api-key': API_KEY,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                console.log('✅ API Connectivity: SUCCESS');
                console.log(`   Received ${Array.isArray(data) ? data.length : 1} pet record(s)`);
                return true;
            } else {
                console.log('❌ API Connectivity: FAILED');
                console.log(`   Status: ${response.status} ${response.statusText}`);
                const errorText = await response.text();
                console.log(`   Error: ${errorText}`);
                return false;
            }
        } catch (error) {
            console.log('❌ API Connectivity: ERROR');
            console.log(`   Error: ${error.message}`);
            return false;
        }
    }

    /**
     * Test all API endpoints
     */
    async function testAllEndpoints() {
        console.log('\nTesting API Endpoints...\n');

        const endpoints = [
            { name: 'Get All Pets', method: 'GET', path: '/api/pets', requiresAuth: false },
            { name: 'Get All Adoptions', method: 'GET', path: '/api/adoptions', requiresAuth: false },
            { name: 'Auth Register', method: 'POST', path: '/api/auth/register', requiresAuth: false },
            { name: 'Auth Login', method: 'POST', path: '/api/auth/login', requiresAuth: false },
        ];

        const results = [];

        for (const endpoint of endpoints) {
            try {
                const options = {
                    method: endpoint.method,
                    headers: {
                        'x-api-key': API_KEY,
                        'Content-Type': 'application/json'
                    }
                };

                // For POST requests, add a minimal body
                if (endpoint.method === 'POST') {
                    if (endpoint.path.includes('register')) {
                        options.body = JSON.stringify({
                            email: 'test@example.com',
                            password: 'Test123!',
                            firstName: 'Test',
                            lastName: 'User',
                            role: 'Public'
                        });
                    } else if (endpoint.path.includes('login')) {
                        options.body = JSON.stringify({
                            email: 'test@example.com',
                            password: 'Test123!'
                        });
                    }
                }

                const response = await fetch(`${API_BASE_URL}${endpoint.path}`, options);
                
                const result = {
                    name: endpoint.name,
                    path: endpoint.path,
                    status: response.status,
                    ok: response.ok,
                    method: endpoint.method
                };

                if (response.ok) {
                    try {
                        const data = await response.json();
                        result.dataReceived = true;
                        result.dataType = Array.isArray(data) ? 'array' : 'object';
                        if (Array.isArray(data)) {
                            result.itemCount = data.length;
                        }
                    } catch (e) {
                        result.dataReceived = false;
                    }
                    results.push(result);
                    console.log(`✅ ${endpoint.name}: ${response.status} ${response.statusText}`);
                } else {
                    const errorText = await response.text();
                    result.error = errorText;
                    results.push(result);
                    console.log(`⚠️  ${endpoint.name}: ${response.status} ${response.statusText}`);
                    if (errorText) {
                        console.log(`   ${errorText.substring(0, 100)}...`);
                    }
                }
            } catch (error) {
                results.push({
                    name: endpoint.name,
                    path: endpoint.path,
                    error: error.message,
                    ok: false
                });
                console.log(`❌ ${endpoint.name}: ERROR - ${error.message}`);
            }
        }

        return results;
    }

    /**
     * Display configuration information
     */
    function displayConfiguration() {
        console.log('\n=== Configuration ===');
        console.log('API Base URL:', API_BASE_URL);
        console.log('API Key:', API_KEY);
        console.log('\nExpected Endpoints:');
        console.log('  - GET  /api/pets');
        console.log('  - GET  /api/pets/{id}');
        console.log('  - POST /api/pets (multipart/form-data)');
        console.log('  - PUT  /api/pets/{id}');
        console.log('  - PATCH /api/pets/{id} (multipart/form-data)');
        console.log('  - DELETE /api/pets/{id}');
        console.log('  - GET  /api/adoptions');
        console.log('  - POST /api/adoptions');
        console.log('  - GET  /api/medicalrecords/pet/{petId}');
        console.log('  - POST /api/medicalrecords');
        console.log('  - POST /api/auth/register');
        console.log('  - POST /api/auth/login');
    }

    /**
     * Run all tests
     */
    async function runTests() {
        displayConfiguration();
        console.log('\n');
        
        const connectivityTest = await testApiConnectivity();
        const endpointTests = await testAllEndpoints();

        console.log('\n=== Test Summary ===');
        console.log(`Connectivity Test: ${connectivityTest ? '✅ PASSED' : '❌ FAILED'}`);
        console.log(`Endpoint Tests: ${endpointTests.filter(r => r.ok).length}/${endpointTests.length} passed`);

        return {
            connectivity: connectivityTest,
            endpoints: endpointTests
        };
    }

    // Export test functions to window for manual execution
    window.apiIntegrationTest = {
        runTests,
        testApiConnectivity,
        testAllEndpoints,
        displayConfiguration
    };

    console.log('\nTest functions are available at: window.apiIntegrationTest');
    console.log('Run: await window.apiIntegrationTest.runTests()\n');

    // Auto-run tests
    runTests().catch(console.error);
})();

