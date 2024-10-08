import { test, expect, APIRequestContext } from '@playwright/test';
import { randomInt } from 'crypto';
import { adminRegistration, delay, registration, RemoveUser, sendFile, Task } from './tools';

//Все тесты в этом файле - поледовательные
test.describe.configure({ mode: 'serial' });

//Тест на стандартное взаимодействие с капчей
test("DefaultCaptchaScript", async({request})=>{
    //Регистрация заказчика и исполнителя
    const customerToken = await registration(request, `DefaultCaptchaScriptCustomer${randomInt(1000)}`, "qwerty", 1);
    const employeerToken = await registration(request, `DefaultCaptchaScriptEmployeer${randomInt(1000)}`, "qwerty", 0);
    try{
        //Отправка капчи пользователя на сервер
        const sendresponse = await sendFile(request, "captures", "./source/captcha.jpg", customerToken);
        const idCustomerCaptcha = await sendresponse.body();

        //Получение капчи исполнителем из пула
        const getCaptchaResponse = await request.get(`emp/captcha`,{
            headers:{
                Authorization:`Bearer ${employeerToken}`
            }
        });
        expect(getCaptchaResponse.ok()).toBeTruthy();
        const idEmpCaptcha = await getCaptchaResponse.body();

        //Получение изображения капчи
        const getImageResponse = await request.get(`emp/captcha/${idEmpCaptcha}`,{
            headers:{
                Authorization:`Bearer ${employeerToken}`
            }
        });
        expect(getImageResponse.ok()).toBeTruthy();
        const result = "result";

        //Отправка исполнителем результата решения
        const postCaptchaResponse = await request.post(`emp/captcha/${idEmpCaptcha}?result=${result}`,{
            headers:{
                Authorization:`Bearer ${employeerToken}`
            }
        });
        expect(postCaptchaResponse.ok()).toBeTruthy();

        //Получаем результат выполнения капчи
        const getResultResponse = await request.get(`captcha/${idCustomerCaptcha}`,{
            headers:{
                Authorization:`Bearer ${customerToken}`
            }
        });
        expect(getResultResponse.ok()).toBeTruthy();
        const resResult = await getResultResponse.text();
        expect(resResult).toEqual(result);
    }
    finally{
        //Удаление пользователей
        await RemoveUser(request, customerToken);
        await RemoveUser(request, employeerToken);
    }

})

//Тест отмены заказа исполнителем
test("CancelCapturesTest", async({request})=>{
       //Регистрация заказчика и исполнителя
       const customerToken = await registration(request, `CancelCaptures${randomInt(1000)}`, "qwerty", 1);
       const employeerToken = await registration(request, `CancelCaptures${randomInt(1000)}`, "qwerty", 0);
       try{
           //Отправка капчи пользователя на сервер
           const sendresponse = await sendFile(request, "captures", "./source/captcha.jpg", customerToken);
           const idCustomerCaptcha = await sendresponse.body();
   
           //Получение капчи исполнителем из пула
           const getCaptchaResponse = await request.get(`emp/captcha`,{
               headers:{
                   Authorization:`Bearer ${employeerToken}`
               }
           });
           expect(getCaptchaResponse.ok()).toBeTruthy();
           const idEmpCaptcha = await getCaptchaResponse.body();
   
           //Получение изображения капчи
           const getImageResponse = await request.get(`emp/captcha/${idEmpCaptcha}`,{
               headers:{
                   Authorization:`Bearer ${employeerToken}`
               }
           });
           expect(getImageResponse.ok()).toBeTruthy();
           const result = "result";
   
           //Отмена выполнения капчи
           const postCaptchaResponse = await request.delete(`emp/captcha/${idEmpCaptcha}`,{
               headers:{
                   Authorization:`Bearer ${employeerToken}`
               }
           });
           expect(postCaptchaResponse.ok()).toBeTruthy();
   
           //Получаем результат выполнения капчи
           const getResultResponse = await request.get(`captcha/${idCustomerCaptcha}`,{
               headers:{
                   Authorization:`Bearer ${customerToken}`
               }
           });
           //Результата нет
           expect(getResultResponse.ok()).toBeFalsy();
       }
       finally{
           //Удаление пользователей
           await RemoveUser(request, customerToken);
           await RemoveUser(request, employeerToken);
       }
});

//Тест отмены заказа заказчиком
test("CancelCustomerCapturesTest", async({request})=>{
    //Регистрация заказчика и админа
    const customerToken = await registration(request, `CancelCustomerCaptures${randomInt(1000)}`, "qwerty", 1);
    const adminToken = await adminRegistration(request, `CancelCustomerCaptures${randomInt(10000)}`);
    try{
        //Отправка капчи пользователя на сервер
        const sendresponse = await sendFile(request, "captures", "./source/captcha.jpg", customerToken);
        const idCustomerCaptcha = await sendresponse.body();

        //Отмена выполнения капчи
        const postCaptchaResponse = await request.delete(`captcha/${idCustomerCaptcha}`,{
            headers:{
                Authorization:`Bearer ${customerToken}`
            }
        });
        expect(postCaptchaResponse.ok()).toBeTruthy();

    //Отправка запроса всех капч от имени администратора
    const response = await request.get("captures", {
        headers: {
            Authorization: `Bearer ${adminToken}`
        }
    });
    expect(response.ok()).toBeTruthy();
    const captures = await response.json() as Task[];

    //Проверка на отсутствие добавленной капчи
    expect(captures.filter(i=>i.id == idCustomerCaptcha).length).toEqual(0);      

    }
    finally{
        //Удаление пользователей
        await RemoveUser(request, customerToken);
    }
});

//Тест показа всех каптч для customer'а
test("ViewAllCapturesForCustomerTest", async({request})=>{
    //Регистрация пользователя
    const token = await registration(request, `ViewAllCapturesForCustomer${randomInt(10000)}`);
    try{
        //Проверка на недоступность капч
        const response = await request.get("captures", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });
        expect(response.status()).toEqual(400);
    }
    finally{
        await RemoveUser(request, token);
    }
});

//Тест регистрации и показа всех каптч для админа
test("ViewAllCapturesForAdminTest", async({request})=>{
    //Регистрация админа и заказчика
    const token = await adminRegistration(request, `ViewAllCapturesForAdmin${randomInt(10000)}`);
    const customerToken = await registration(request, `ViewAllCapturesForAdmin${randomInt(10000)}`, "qwerty", 1);
    try{
        //Отправка капчи пользователя на сервер
        const sendresponse = await sendFile(request, "captures", "./source/captcha.jpg", customerToken);
        const idCaptcha = await sendresponse.body();

        //Отправка запроса всех капч от имени администратора
        const response = await request.get("captures", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });
        expect(response.ok()).toBeTruthy();
        const captures = await response.json() as Task[];

        //Проверка на наличие добавленной капчи
        expect(captures.filter(i=>i.id == idCaptcha).length).toEqual(1);            
    }
    finally{
        //Удаление пользователей
        await RemoveUser(request, customerToken);
        await RemoveUser(request, token);
    }
});

//Тест удаление всех капч пользователя с его удалением
test("NotViewDeletedUsersCapturesForAdminTest", async({request})=>{
    //Регистрация админа и заказчика
    const token = await adminRegistration(request, `NotViewDeletedUsersCapturesForAdminTest${randomInt(10000)}`);
    const customerToken = await registration(request, `NotViewDeletedUsersCapturesForAdminTest${randomInt(10000)}`, "qwerty", 1);
    try{
        //Отправка капчи пользователя на сервер
        const sendresponse = await sendFile(request, "captures", "./source/captcha.jpg", customerToken);
        const idCaptcha = await sendresponse.body();

        //Удаление заказчика
        await RemoveUser(request, customerToken);

        //Отправка запроса всех капч от имени администратора
        const response = await request.get("captures", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });
        expect(response.ok()).toBeTruthy();
        const captures = await response.json() as Task[];

        //Проверка на отсутствие капчи удалённого пользователя
        expect(captures.filter(i=>i.id == idCaptcha).length).toEqual(0);            
    }
    finally{
        //Удаление админа
        await RemoveUser(request, token);
    }
});

