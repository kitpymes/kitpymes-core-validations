using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kitpymes.Core.Validations.Abstractions.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kitpymes.Core.Validations.Tests.Validators;

// Cada cadena que empieza en Validator.AddRule tiene sus propios errores.
// Hasta la 2.1.0 todas compartían una lista estática: estos tests fallan con ella.
[TestClass]
public class ValidatorRuleTests
{
    private const string A = "Error A";
    private const string B = "Error B";

    [TestMethod]
    public void Dos_cadenas_no_comparten_errores()
    {
        var mala = Validator.AddRule(null, x => x.IsNullOrEmptyWithMessage(A));
        var buena = Validator.AddRule("valor", x => x.IsNullOrEmptyWithMessage(B));

        Assert.IsFalse(mala.IsValid);
        Assert.IsTrue(buena.IsValid);
    }

    [TestMethod]
    public void Una_cadena_abandonada_no_contamina_la_siguiente()
    {
        _ = Validator.AddRule(null, x => x.IsNullOrEmptyWithMessage(A));

        var buena = Validator.AddRule("valor", x => x.IsNullOrEmptyWithMessage(B));

        Assert.IsTrue(buena.IsValid);
        buena.Throw();
    }

    [TestMethod]
    public void El_Throw_de_una_cadena_no_lanza_ni_borra_los_errores_de_otra()
    {
        var mala = Validator.AddRule(null, x => x.IsNullOrEmptyWithMessage(A));
        var buena = Validator.AddRule("valor", x => x.IsNullOrEmptyWithMessage(B));

        buena.Throw();

        var exception = Assert.ThrowsExactly<ValidationsException>(() => mala.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Cadenas_en_paralelo_reciben_sólo_sus_errores()
    {
        var fallos = new ConcurrentBag<string>();

        Parallel.For(0, 2000, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
        {
            var propio = $"Error {i}";

            if (i % 2 == 0)
            {
                var rule = Validator.AddRule(null, x => x.IsNullOrEmptyWithMessage(propio));

                try
                {
                    rule.Throw();
                    fallos.Add($"{i}: no lanzó");
                }
                catch (ValidationsException exception) when (exception.Message == propio)
                {
                }
                catch (Exception exception)
                {
                    fallos.Add($"{i}: {exception.Message}");
                }
            }
            else
            {
                var rule = Validator.AddRule("valor", x => x.IsNullOrEmptyWithMessage(propio));

                try
                {
                    rule.Throw();
                }
                catch (Exception exception)
                {
                    fallos.Add($"{i}: lanzó {exception.Message}");
                }
            }
        });

        Assert.IsTrue(fallos.IsEmpty, string.Join(" | ", fallos));
    }

    [TestMethod]
    public void StopFirstError_en_medio_de_la_cadena_vale_para_toda_la_cadena()
    {
        var rule = Validator
            .AddRule(null, x => x.IsNullOrEmptyWithMessage(A))
            .StopFirstError()
            .AddRule(null, x => x.IsNullOrEmptyWithMessage(B));

        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Sin_StopFirstError_salen_todos_los_errores_de_la_cadena_en_orden()
    {
        var rule = Validator
            .AddRule(null, x => x.IsNullOrEmptyWithMessage(A))
            .AddRule(() => true, B);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual($"{A}, {B}", exception.Message);
    }

    [TestMethod]
    public void StopFirstError_false_devuelve_todos_los_errores()
    {
        var rule = Validator
            .AddRule(() => true, A)
            .StopFirstError()
            .StopFirstError(false)
            .AddRule(() => true, B);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual($"{A}, {B}", exception.Message);
    }

    [TestMethod]
    public void AddRule_con_condición_falsa_o_nula_no_agrega_error()
    {
        var rule = Validator
            .AddRule(() => false, A)
            .AddRule(null!, B);

        Assert.IsTrue(rule.IsValid);
    }

    [TestMethod]
    public void Throw_vacía_la_cadena()
    {
        var rule = Validator.AddRule(() => true, A);

        _ = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());

        Assert.IsTrue(rule.IsValid);
        rule.Throw();
    }

    [TestMethod]
    public void Una_cadena_creada_dentro_de_las_opciones_de_otra_no_se_lleva_sus_errores()
    {
        ValidatorRule? interior = null;

        var exterior = Validator.AddRule(null, x =>
        {
            interior = Validator.AddRule("valor", y => y.IsNullOrEmptyWithMessage(B));
            x.IsNullOrEmptyWithMessage(A);
        });

        Assert.IsNotNull(interior);
        Assert.IsTrue(interior!.IsValid);
        var exception = Assert.ThrowsExactly<ValidationsException>(() => exterior.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Add_fuera_de_las_opciones_de_una_regla_lanza_InvalidOperationException()
    {
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => ValidatorRule.Add(() => true, A));
        _ = Assert.ThrowsExactly<InvalidOperationException>(() => new ValidatorRuleOptions(null).IsNullOrEmptyWithMessage(A));
    }

    [TestMethod]
    public void Add_después_de_una_excepción_en_las_opciones_sigue_fuera_de_cadena()
    {
        _ = Assert.ThrowsExactly<NotSupportedException>(() => Validator.AddRule(null, _ => throw new NotSupportedException()));

        _ = Assert.ThrowsExactly<InvalidOperationException>(() => ValidatorRule.Add(() => true, A));
    }

    [TestMethod]
    public void ThrowAsync_termina_sin_esperar_con_el_error_en_la_tarea()
    {
        var rule = Validator.AddRule(() => true, A);

        var tarea = rule.ThrowAsync();

        Assert.IsTrue(tarea.IsFaulted);
        Assert.IsInstanceOfType<ValidationsException>(tarea.Exception!.InnerException);
        Assert.AreEqual(A, tarea.Exception!.InnerException!.Message);
    }

    [TestMethod]
    public void ThrowAsync_sin_errores_termina_sin_esperar()
    {
        var tarea = Validator.AddRule(() => false, A).ThrowAsync();

        Assert.IsTrue(tarea.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task ThrowAsync_de_tipo_lanza_ese_tipo()
    {
        var rule = Validator.AddRule(() => true, A);

        var exception = await Assert.ThrowsExactlyAsync<ApplicationException>(() => rule.ThrowAsync<ApplicationException>());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Throw_de_tipo_lanza_ese_tipo()
    {
        var rule = Validator.AddRule(() => true, A);

        var exception = Assert.ThrowsExactly<ApplicationException>(() => rule.Throw<ApplicationException>());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Una_cadena_en_otro_hilo_no_ve_la_cadena_que_se_está_configurando()
    {
        Exception? enOtroHilo = null;

        var rule = Validator.AddRule(null, x =>
        {
            var hilo = new Thread(() =>
            {
                try
                {
                    ValidatorRule.Add(() => true, B);
                }
                catch (Exception exception)
                {
                    enOtroHilo = exception;
                }
            });
            hilo.Start();
            hilo.Join();
            x.IsNullOrEmptyWithMessage(A);
        });

        Assert.IsInstanceOfType<InvalidOperationException>(enOtroHilo);
        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public async Task Opciones_async_no_tumban_el_proceso_y_el_error_llega_a_su_cadena()
    {
        using var hecho = new SemaphoreSlim(0);

        var rule = Validator.AddRule(null, async x =>
        {
            await Task.Yield();
            x.IsNullOrEmptyWithMessage(A);
            hecho.Release();
        });

        Assert.IsTrue(await hecho.WaitAsync(TimeSpan.FromSeconds(10)));
        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Reglas_en_paralelo_dentro_de_las_opciones_llegan_todas_a_su_cadena()
    {
        var rule = Validator.AddRule(null, x => Parallel.For(0, 500, _ => x.IsNullOrEmptyWithMessage(A)));

        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(500, exception.Message.Split(", ").Length);
    }

    [TestMethod]
    public void Con_StopFirstError_las_reglas_posteriores_al_primer_error_no_se_evalúan()
    {
        var evaluada = false;

        var rule = Validator
            .AddRule(() => true, A)
            .StopFirstError()
            .AddRule(
                () =>
                {
                    evaluada = true;
                    return true;
                },
                B);

        Assert.IsFalse(evaluada);
        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(A, exception.Message);
    }

    [TestMethod]
    public void Con_StopFirstError_sin_errores_previos_sí_se_evalúa()
    {
        var rule = Validator
            .AddRule(() => false, A)
            .StopFirstError()
            .AddRule(() => true, B);

        var exception = Assert.ThrowsExactly<ValidationsException>(() => rule.Throw());
        Assert.AreEqual(B, exception.Message);
    }

    [TestMethod]
    public void Las_opciones_de_una_cadena_pueden_añadir_a_otra_cadena_existente()
    {
        var otra = Validator.AddRule(() => false, B);

        var una = Validator.AddRule(null, x =>
        {
            otra.AddRule(null, y => y.IsNullOrEmptyWithMessage(B));
            x.IsNullOrEmptyWithMessage(A);
        });

        Assert.AreEqual(A, Assert.ThrowsExactly<ValidationsException>(() => una.Throw()).Message);
        Assert.AreEqual(B, Assert.ThrowsExactly<ValidationsException>(() => otra.Throw()).Message);
    }

    [TestMethod]
    public void Una_cadena_interior_que_lanza_no_rompe_la_exterior()
    {
        var exterior = Validator.AddRule(null, x =>
        {
            _ = Assert.ThrowsExactly<NotSupportedException>(() => Validator.AddRule(null, _ => throw new NotSupportedException()));
            x.IsNullOrEmptyWithMessage(A);
        });

        Assert.AreEqual(A, Assert.ThrowsExactly<ValidationsException>(() => exterior.Throw()).Message);
    }

    [TestMethod]
    public void Add_público_dentro_de_las_opciones_escribe_en_esa_cadena()
    {
        var rule = Validator.AddRule(null, _ => ValidatorRule.Add(() => true, A));
        var otra = Validator.AddRule(null, _ => ValidatorRule.Add(() => false, B));

        Assert.IsTrue(otra.IsValid);
        Assert.AreEqual(A, Assert.ThrowsExactly<ValidationsException>(() => rule.Throw()).Message);
    }

    [TestMethod]
    public void StopFirstError_al_final_de_la_cadena_deja_sólo_el_primer_error()
    {
        var rule = Validator
            .AddRule(() => true, A)
            .AddRule(() => true, B)
            .StopFirstError();

        Assert.AreEqual(A, Assert.ThrowsExactly<ValidationsException>(() => rule.Throw()).Message);
    }
}
