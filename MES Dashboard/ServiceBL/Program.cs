using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});


var app = builder.Build();

var connectionString =  builder.Configuration.GetConnectionString("dbcon");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// It needs to be scheduled.
app.MapGet("/process_admin_dashboard", () =>
{
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "sp_dash_process_admin";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.ExecuteNonQuery();
            connection.Close(); 
        }
        return new[] { "Success" };
    }
    catch (Exception ex)
    {
        return new[] { ex.Message };
    }
        
});

app.MapGet("/admin_dashboard", (DateTime dt) =>
{
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "sp_dash_admin";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@dt", dt);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            dynamic jsonObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
            return jsonObj;
        }
    }
    catch (Exception ex)
    {
        return new[] { ex.Message };
    }
});


app.MapGet("/hourly_dashboard", (DateTime dt, string shift) =>
{
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "sp_dash_hourly";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;          
            cmd.Parameters.AddWithValue("@dt", dt);
            cmd.Parameters.AddWithValue("@shift", shift);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            dynamic jsonObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);

            return jsonObj ;
        }        
    }
    catch (Exception ex)
    {
        return new[] { ex.Message };
    }
});

app.MapGet("/line_dashboard", () =>
{
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "sp_dash_line";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;            
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            dynamic jsonObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
            return jsonObj;
        }
    }
    catch (Exception ex)
    {
        return new[] { ex.Message };
    }
});

// Need to check.
app.MapGet("/plan_dashboard", (DateTime dt) =>
{
    try
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = "sp_dash_planned";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            dt = new DateTime(2021, 12, 14);
            cmd.Parameters.AddWithValue("@dt", dt);            
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            string json = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            dynamic jsonObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);

            return jsonObj;
        }
    }
    catch (Exception ex)
    {
        return new[] { ex.Message };
    }
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}