import { Context, RetrieveMultipleResponse } from '../../types/PcfCoreTypes';

export const fetchGlobalParameter = async (
  context: Context,
  parameterName: string | null
): Promise<RetrieveMultipleResponse> => {
  const globalParameter = await context.webAPI.retrieveMultipleRecords(
    'alt_globalparameter',
    '?fetchXml=' + getGlobalParameterFetchXml(parameterName)
  );

  return globalParameter;
};

export const fetchByXml = async (
  context: Context,
  fetchXml: string,
  entityName: string
) => {
  return await context.webAPI.retrieveMultipleRecords(
    entityName,
    '?fetchXml=' + fetchXml
  );
};

const getGlobalParameterFetchXml = (parameterName: string | null): string => {
  const name: string =
    parameterName !== null ? parameterName : 'DuplicatesSearchConfiguration';

  const fetchXml: string = `
      <fetch version="1.0" mapping="logical" distinct="false">
        <entity name="alt_globalparameter">
          <attribute name="alt_value"/>
          <filter type="and">
            <condition attribute="alt_name" operator="eq" value="${name}"/>
          </filter>
        </entity>
      </fetch>
    `;

  return fetchXml;
};
