using System;
using System.Collections.Generic;

namespace Zhichkin.Metadata.Model
{
    public enum TablePurpose
    {
        /// <summary>The main table to store data.</summary>
        Main,
        Constants,
        Totals,
        Turnovers,
        TotalsByAccounts,
        TotalsByAccountsWithExtDim,
        TotalsBetweenAccounts,
        TotalsSliceFirst,
        TotalsSliceLast,
        ExtDimensionTypes,
        ExtDimensionsValues,
        DisplacingCalculationTypes,
        DisplacementOrder,
        LeadingCalculationTypes,
        BaseCalculationTypes,
        ActionPeriods,
        SequenceBoundaries,
        RoutePoints,
        /// <summary>The table part of entity.</summary>
        TabularSection,
        /// <summary>The table for tracking changes.</summary>
        ChangeRecord,
        ConstantsChangeRecord,
        ConfigChangeRecord,
        ConfigExtPropertiesChangeRecord,
        AccumRgSt,
        AccumRgDl,
        AccumRgBf,
        AccumRgAggOpt,
        AccumRgAggDims,
        AccumRgAggGrid,
        SystemSettings,
        CommonSettings,
        ReportsSettings,
        ReportVariants,
        FrmDtSettings,
        AccumRegAgg,
        UsersHistoryStorage,
        Task,
        AccountRegistersOptionsTable,
        AccumulationRegistersOptionsTable,
        AccountRegisterTotalsOptions,
        AccumulationRegisterOptionsTable,
        InformationRegisterTotalsOptions,
        ChartsOfAccountsOptions,
        ChartsOfCharacteristicTypesOptions,
        ChartsOfCalculationTypesOptions,
        AccumRgAggOptions,
        ReferenceOptions,
        InitializedPredefinedDataInChartOfCharacteristicTypes,
        InitializedPredefinedDataInChartOfAccounts,
        InitializedPredefinedDataInChartOfCalculationTypes,
        /// <summary>Таблица проинициализированных предопределенных данных справочника.</summary>
        InitializedPredefinedDataInCatalog,
        AccumulationRegisterMeasureDictionary
    }
    public sealed class TablePurposes
    {
        private static readonly TablePurposes singelton = new TablePurposes();

        private TablePurposes() { }

        public static TablePurposes Lookup
        {
            get { return singelton; }
        }

        public TablePurpose this[string name] { get { return tablePurposes[name]; } }

        private static Dictionary<string, TablePurpose> tablePurposes = new Dictionary<string, TablePurpose>()
        {
            { "Основная", TablePurpose.Main }, //основная
            { "Константы", TablePurpose.Constants }, //константы
            { "Итоги", TablePurpose.Totals }, //итоги
            { "Обороты", TablePurpose.Turnovers }, //обороты
            { "ИтогиПоСчетам", TablePurpose.TotalsByAccounts }, //итоги по счетам
            { "ИтогиПоСчетамССубконто", TablePurpose.TotalsByAccountsWithExtDim }, //итоги по счетам с субконто
            { "ИтогиМеждуСчетами", TablePurpose.TotalsBetweenAccounts }, //итоги между счетами
            { "ИтогиСрезПервых", TablePurpose.TotalsSliceFirst }, //итоги срез первых регистра сведений
            { "ИтогиСрезПоследних", TablePurpose.TotalsSliceLast }, //итоги срез последних регистра сведений
            { "ВидыСубконто", TablePurpose.ExtDimensionTypes }, //виды субконто
            { "ЗначенияСубконто", TablePurpose.ExtDimensionsValues }, //значения субконто
            { "ВытесняющиеВидыРасчета", TablePurpose.DisplacingCalculationTypes }, //вытесняющие виды расчета
            { "ПорядокВытеснения", TablePurpose.DisplacementOrder }, //порядок вытеснения
            { "ВедущиеВидыРасчета", TablePurpose.LeadingCalculationTypes }, //ведущие виды расчета
            { "БазовыеВидыРасчета", TablePurpose.BaseCalculationTypes }, //базовые виды расчета
            { "ПериодыДействия", TablePurpose.ActionPeriods }, //периоды действия
            { "ГраницыПоследовательности", TablePurpose.SequenceBoundaries }, //границы последовательности
            { "ТочкиМаршрута", TablePurpose.RoutePoints }, //точки маршрута бизнес–процесса
            { "ТабличнаяЧасть", TablePurpose.TabularSection }, //табличная часть
            { "РегистрацияИзменений", TablePurpose.ChangeRecord }, //регистрация изменений
            { "РегистрацияИзмененийКонстант", TablePurpose.ConstantsChangeRecord }, //регистрация изменений констант
            { "РегистрацияИзмененийКонфигурации", TablePurpose.ConfigChangeRecord }, //регистрация изменений конфигурации
            { "РегистрацияИзмененийВнешнихСвойствКонфигурации", TablePurpose.ConfigExtPropertiesChangeRecord }, //регистрация изменений внешних свойств конфигурации
            { "СтатистикаЗапросов", TablePurpose.AccumRgSt }, //таблица статистики запросов
            { "НовыеОбороты", TablePurpose.AccumRgDl }, //таблица новых оборотов по регистру
            { "БуферОборотов", TablePurpose.AccumRgBf }, //таблица буфера оборотов по регистру
            { "НастройкиРежимаАгрегатовРегистровНакопления", TablePurpose.AccumRgAggOpt }, //таблица настроек режима агрегатов регистра накопления
            { "КодыИзмеренийАгрегатовРегистровНакопления", TablePurpose.AccumRgAggDims }, //таблица кодов измерений регистра накопления в агрегатах
            { "СписокАгрегатовРегистровНакопления", TablePurpose.AccumRgAggGrid }, //таблица списка агрегатов регистра накопления
            { "ХранилищеСистемныхНастроек", TablePurpose.SystemSettings }, //хранилище системных настроек
            { "ХранилищеОбщихНастроек", TablePurpose.CommonSettings }, //хранилище общих настроек
            { "ХранилищеПользовательскихНастроекОтчетов", TablePurpose.ReportsSettings }, //хранилище пользовательских настроек отчетов
            { "ХранилищеВариантовОтчетов", TablePurpose.ReportVariants }, //хранилище вариантов отчета
            { "ХранилищеНастроекДанныхФорм", TablePurpose.FrmDtSettings }, //хранилище настоек данных форм
            { "АгрегатРегистраНакопления", TablePurpose.AccumRegAgg }, //таблица агрегата регистра накопления
            { "ИсторияРаботыПользователей", TablePurpose.UsersHistoryStorage }, //история работы пользователей
            { "Задача", TablePurpose.Task }, //таблица задач бизнесс процесса
            { "НастройкиХраненияИтоговРегистровБухгалтерии", TablePurpose.AccountRegistersOptionsTable }, //таблица настроек хранения итогов регистров бухгалтерии
            { "НастройкиХраненияИтоговРегистровНакопления", TablePurpose.AccumulationRegistersOptionsTable }, //таблица настроек хранения итогов регистров накопления
            { "НастройкиХраненияИтоговРегистраБухгалтерии", TablePurpose.AccountRegisterTotalsOptions }, //таблица настроек хранения итогов регистра бухгалтерии
            { "НастройкиХраненияИтоговРегистраНакопления", TablePurpose.AccumulationRegisterOptionsTable }, //таблица настроек хранения итогов регистра накопления
            { "НастройкиХраненияИтоговРегистраСведений", TablePurpose.InformationRegisterTotalsOptions }, //таблица настроек использования итогов среза первых и среза последних регистра сведений
            { "НастройкиПлановСчетов", TablePurpose.ChartsOfAccountsOptions }, //настройки планов счетов
            { "НастройкиПлановВидовХарактеристик", TablePurpose.ChartsOfCharacteristicTypesOptions }, //настройки планов видов характеристик
            { "НастройкиПлановВидовРасчетов", TablePurpose.ChartsOfCalculationTypesOptions }, //настройки планов видов расчета
            { "НастройкиСпискаАгрегатов", TablePurpose.AccumRgAggOptions }, //настройки списка агрегатов
            { "НастройкиСправочников", TablePurpose.ReferenceOptions }, //настройки справочников
            { "ИнициализированныеПредопределенныеДанныеПланаВидовХарактеристик", TablePurpose.InitializedPredefinedDataInChartOfCharacteristicTypes }, //таблица проинициализированных предопределенных данных плана видов характеристик
            { "ИнициализированныеПредопределенныеДанныеПланаСчетов", TablePurpose.InitializedPredefinedDataInChartOfAccounts }, //таблица инициализированных предопределенных данных плана счетов
            { "ИнициализированныеПредопределенныеДанныеПланаВидовРасчета", TablePurpose.InitializedPredefinedDataInChartOfCalculationTypes }, //таблица проинициализированных элементов плана расчетов
            { "ИнициализированныеПредопределенныеДанныеСправочника", TablePurpose.InitializedPredefinedDataInCatalog }, //таблица проинициализированных предопределенных данных справочника
            { "СловарьИзмеренияРегистраНакопления", TablePurpose.AccumulationRegisterMeasureDictionary }
        };
    }
}
