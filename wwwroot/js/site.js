
const connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();
connection.on("ReloadProduct", (data) => {
    location.reload();
})

connection.start();