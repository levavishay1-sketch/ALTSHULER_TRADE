import * as React from 'react';
import { GridBody } from './components/grid-components/GridBody';
import { IInputs, IOutputs } from './generated/ManifestTypes';
import { IGridProps } from './utilities/interfaces';
import { RetrieveMultipleResponse } from './types/PcfCoreTypes';
import { fetchGlobalParameter } from './utilities/common/FetchXmlUtils';

export class DuplicateDetectionPCF
  implements ComponentFramework.ReactControl<IInputs, IOutputs>
{
  private theComponent: ComponentFramework.ReactControl<IInputs, IOutputs>;
  private notifyOutputChanged: () => void;

  constructor() {}

  public init(
    context: ComponentFramework.Context<IInputs>,
    notifyOutputChanged: () => void,
    state: ComponentFramework.Dictionary
  ): void {
    this.notifyOutputChanged = notifyOutputChanged;
    context.mode.trackContainerResize(true);
  }

  public updateView(
    context: ComponentFramework.Context<IInputs>
  ): React.ReactElement {
    const props: IGridProps = {
      context: context,
      containerWidth: context.mode.allocatedWidth
    };
    return React.createElement(GridBody, props);
  }

  public getOutputs(): IOutputs {
    return {};
  }

  public destroy(): void {}
}
